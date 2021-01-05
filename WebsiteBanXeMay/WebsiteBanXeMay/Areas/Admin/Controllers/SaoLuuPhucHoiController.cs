using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class SaoLuuPhucHoiController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();

        // GET: Admin/SaoLuuPhucHoi
        public ActionResult Index(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstBackup(Constant.DATABASE),
                CurrentPage = Trang
            };
            return View(Model);
        }


        [HttpPost]
        public JsonResult SaoLuu(bool flag)
        {
            var msg = new JMessage() { error = false, title = "" };
            //check tồn tại backup devices 
            bool check = true;
            if (Check_Create_Backup_Devices(Constant.DATABASE) == false)
            {
                // Chưa có backup devices ==>  Tạo 
                if (Exec_Create_Backup_Devices(Constant.DATABASE) == -1)
                {
                    check = true;
                }
                else 
                {
                    check = false;
                    msg.title = "Tạo Backup devices thất bại";
                    msg.error = true;
                }
            }

            if (check == true)
            {
                if (BackupDatabase(flag, Constant.DATABASE) == -1)
                {
                    var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                    var objBackup = lstBackup(Constant.DATABASE).FirstOrDefault();
                    string strQuery = string.Format("UPDATE msdb.dbo.backupset SET description = '{0}' where backup_set_id = {1}", objTaiKhoan.EMAIL, objBackup.backup_set_id);
                    int res = DB.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, strQuery);
                    msg.title = "Sao lưu thành công";
                    msg.error = false;
                }
                else
                {
                    msg.title = "Sao lưu thất bại";
                    msg.error = true;
                }
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult PhucHoi(int position)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (RestoreDatabase(Constant.DATABASE, position) == -1)
            {
                msg.title = "Phục hồi thành công";
                msg.error = false;
            }
            else
            {
                msg.title = "Phục hồi thất bại";
                msg.error = true;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }


        //======================= Truy vấn database =============================
        //Kiểm tra đã có  backup device chưa
        private bool Check_Create_Backup_Devices(string strDatabaseName)
        {
            try
            {
                string strQuery = string.Format("select name from sys.sysdevices");
                var lstBackupDevices = DB.Database.SqlQuery<string>(strQuery).ToList();//danh sách backup device
                foreach (string strBackup_Device in lstBackupDevices)
                {
                    if (strBackup_Device.Contains(strDatabaseName) == true)//Kiểm tra backup device đã tồn tại chưa 
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        //Exec tạo backup device
        private int Exec_Create_Backup_Devices(string strDatabaseName)
        {
            var pathUpload = Path.Combine(Server.MapPath("~/Assets/upload/backupRestore/"));
            if (!Directory.Exists(pathUpload)) Directory.CreateDirectory(pathUpload);

            string strQuery = string.Format("EXEC sp_addumpdevice '{0}','DEVICE_{1}','{2}.bak'",
                "DISK", strDatabaseName, pathUpload + "DEVICE_" + strDatabaseName);
            int res = DB.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, strQuery);
            return res;
        }


        // Danh sách bản backup
        private IEnumerable<SaoLuuPhucHoiViewModel> lstBackup(string strDatabaseName)
        {
            var strQuery = string.Format("SELECT  backup_set_id,   position, description, backup_start_date  FROM  msdb.dbo.backupset " +
                                          " WHERE database_name = '{0}'  AND type = 'D' AND" +
                                          "  backup_set_id >=" +
                                          "         (SELECT MAX(backup_set_id) FROM     msdb.dbo.backupset" +
                                          "            WHERE media_set_id =" +
                                          "         (SELECT  MAX(media_set_id)" +
                                          "              FROM msdb.dbo.backupset" +
                                          "                   WHERE database_name = '{0}'  AND type = 'D')" +
                                          "         AND position = 1" +
                                          " ) " +
                                          "ORDER BY position DESC", strDatabaseName);
            var lstBackup = DB.Database.SqlQuery<SaoLuuPhucHoiViewModel>(strQuery).ToList();
            return lstBackup;
        }


        // Sao lưu dữ liệu
        private int BackupDatabase(bool flag, string strDatabaseName)
        {
            string strQuery;
            strQuery = string.Format("BACKUP DATABASE {0} TO DEVICE_{0} ", strDatabaseName);
            if (flag == true)
            {
                strQuery += " WITH INIT";//xóa các bản sao lưu cũ để lưu bản sao mới
            }
            int res = DB.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, strQuery);
            return res;
        }


        //Phục hồi dữ liệu
        private int RestoreDatabase(string strDatabaseName, int position)
        {
            string strQuery;
            //Đóng kết nối
            if (DB.Database.Connection.State == ConnectionState.Open)
            {
                DB.Database.Connection.Close();
            }
            //set database 1 người dùng và đứng ở connect đó mở 1 database mới là tempdb đê thực hiện việc restore sau này
            strQuery = string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE USE tempdb ;", strDatabaseName);
            strQuery += string.Format("RESTORE DATABASE {0} FROM  DEVICE_{0}  WITH FILE= {1}, REPLACE ;", strDatabaseName, position);
            strQuery += string.Format("ALTER DATABASE {0} SET MULTI_USER ;", strDatabaseName);

            int res = DB.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,strQuery);
            return res;
        }
    }
}
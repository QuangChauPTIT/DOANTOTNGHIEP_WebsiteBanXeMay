(function ($) {
	const formatter = new Intl.NumberFormat('VI', {
		style: 'currency',
		currency: 'vnd',
		minimumFractionDigits: 0
	})
	$.fn.simpleMoneyFormat = function () {
		this.each(function (index, el) {
			var value = null;
			// get value

			value = $(el).text().trim();
			if (value.indexOf(".") != -1) {
				return false;
			}
			var result = formatter.format(value).replace(/,/g, ".");
			$(el).empty().text(result);
		});
	}
}(jQuery));	
﻿#nullable disable
namespace KsSelect.Samples.Models;

public class BookLocalizationJoin
{
	public Book Book { get; set; }

	public BookLocalization Localization { get; set; }
}
#nullable restore
using AutoMapper;
using KsSelect.Samples.Repositories;
using KsSelect.Samples.Repositories.Filter;
using KsSelect.Samples.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KsSelect.Samples.Controllers;

public class BooksController : Controller
{
	private readonly IBookRepository _bookRepository;
	private readonly IMapper _mapper;

	public BooksController(IBookRepository bookRepository, IMapper mapper)
	{
		_bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public IActionResult Index()
	{
		var filter = new BookFilter
		{
			IncludeAuthorName = true,
			IncludeAuthorInfo = true
		};
		var items = _bookRepository.Query(filter).Select(x => _mapper.Map<BookViewModel>(x));
		return View(items);
	}
}

using AutoMapper;
using KsSelect.Samples.Models;
using KsSelect.Samples.ViewModels;

namespace KsSelect.Samples;

public class AuthorProfile : Profile
{
	public AuthorProfile()
	{
		CreateMap<Author, AuthorViewModel>();
		CreateMap<AuthorInfo, AuthorInfoViewModel>();
	}
}

public class BookProfile : Profile
{
	public BookProfile()
	{
		CreateMap<Book, BookViewModel>();
	}
}

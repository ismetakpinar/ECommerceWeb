using AutoMapper;
using ECommerceAPI.Dto.Dtos.AddressDtos;
using ECommerceAPI.Dto.Dtos.CategoryDtos;
using ECommerceAPI.Dto.Dtos.CustomerDtos;
using ECommerceAPI.Dto.Dtos.OrderDtos;
using ECommerceAPI.Dto.Dtos.ProductDtos;
using ECommerceAPI.Entity.Entities;
namespace ECommerceAPI.Business.Mapping;

public class GeneralMapping : Profile
{
    public GeneralMapping()
    {
        CreateMap<Product, CreateProductDto>().ReverseMap();

        CreateMap<Product, ProductUpdateDto>().ReverseMap();

        CreateMap<Product, ProductListDto>()
    .ForMember(
        dest => dest.CategoryName,
        opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<Product, ProductDetailDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<RegisterDto, AppUser>()
            .ForMember(
                destination => destination.PasswordHash,
                option => option.Ignore())
            .ForMember(
                destination => destination.Role,
                option => option.Ignore());


        CreateMap<Category, CreateCategoryDto>().ReverseMap();

        CreateMap<Category, UpdateCategoryDto>().ReverseMap();

        CreateMap<Category, CategoryListDto>();

        CreateMap<Category, CategoryDetailDto>()
            .ForMember(
                destination => destination.ProductCount,
                options => options.MapFrom(
                source => source.Products.Count));


        CreateMap<Customer, UpdateCustomerDto>().ReverseMap();

        CreateMap<Customer, CustomerListDto>();

        CreateMap<Customer, CustomerDetailDto>()
    .ForMember(
        destination => destination.Email,
        options => options.MapFrom(source => source.AppUser.Email))
    .ForMember(
        destination => destination.AddressCount,
        options => options.MapFrom(source => source.Addresses.Count))
    .ForMember(
        destination => destination.OrderCount,
        options => options.MapFrom(source => source.Orders.Count));

        CreateMap<Address, CreateAddressDto>().ReverseMap();

        CreateMap<Address, UpdateAddressDto>().ReverseMap();

        CreateMap<Address, AddressListDto>()
            .ForMember(
                dest => dest.CustomerName,
                opt => opt.MapFrom(src =>
                    src.Customer.FirstName + " " + src.Customer.LastName));

        CreateMap<Address, AddressDetailDto>()
            .ForMember(
                dest => dest.CustomerName,
                opt => opt.MapFrom(src =>
                    src.Customer.FirstName + " " + src.Customer.LastName));



        CreateMap<Order, OrderListDto>()
            .ForMember(
                dest => dest.CustomerName,
                opt => opt.MapFrom(src =>
                    src.Customer.FirstName + " " + src.Customer.LastName));

        CreateMap<Order, OrderDetailDto>()
            .ForMember(
                dest => dest.CustomerName,
                opt => opt.MapFrom(src =>
                    src.Customer.FirstName + " " + src.Customer.LastName));



    }
}
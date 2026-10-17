using ECommerceAPI.Business.Abstract;
using ECommerceAPI.Business.Concrete;
using ECommerceAPI.DataAccess.Specifications;
using ECommerceAPI.DataAccess.UnitOfWorks;
using ECommerceAPI.Dto.Dtos.CartDtos;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using ECommerceAPI.Entity.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace ECommerceAPI.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock;

    private readonly Mock<IUnitOfWork>
        _unitOfWorkMock;

    private readonly Mock<ICustomerService>
        _customerServiceMock;

    private readonly Mock<IGenericRepository<Product>>
        _productRepositoryMock;

    private readonly Mock<IGenericRepository<Cart>>
        _cartRepositoryMock;

    private readonly Mock<IGenericRepository<CartItem>>
        _cartItemRepositoryMock;

    private readonly Mock<IGenericRepository<Address>>
        _addressRepositoryMock;

    private readonly Mock<IGenericRepository<Order>>
        _orderRepositoryMock;

    private readonly Mock<IGenericRepository<OrderDetail>>
        _orderDetailRepositoryMock;

    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _unitOfWorkMock =
            new Mock<IUnitOfWork>();

        _customerServiceMock =
            new Mock<ICustomerService>();

        _productRepositoryMock =
            new Mock<IGenericRepository<Product>>();

        _cartRepositoryMock =
            new Mock<IGenericRepository<Cart>>();

        _cartItemRepositoryMock =
            new Mock<IGenericRepository<CartItem>>();

        _addressRepositoryMock =
            new Mock<IGenericRepository<Address>>();

        _orderRepositoryMock =
            new Mock<IGenericRepository<Order>>();

        _orderDetailRepositoryMock =
            new Mock<IGenericRepository<OrderDetail>>();

        _unitOfWorkMock
            .Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Cart>())
            .Returns(_cartRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<CartItem>())
            .Returns(_cartItemRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Address>())
            .Returns(_addressRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.Repository<OrderDetail>())
            .Returns(_orderDetailRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cartService = new CartService(
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _customerServiceMock.Object);
    }

    [Fact]
    public async Task AddToCartAsync_QuantityIsZero_ThrowsBadRequestException()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 0
        };

        // Act
        Func<Task> act = async () =>
            await _cartService.AddToCartAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Ürün miktarı 0'dan büyük olmalıdır.");

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_QuantityIsNegative_ThrowsBadRequestException()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = -2
        };

        // Act
        Func<Task> act = async () =>
            await _cartService.AddToCartAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Ürün miktarı 0'dan büyük olmalıdır.");

        _customerServiceMock.Verify(
            x => x.GetByAppUserIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 50,
            Quantity = 2
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _productRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.AddToCartAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");

        _cartRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_RequestedQuantityExceedsStock_ThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 11
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var product = CreateProduct(
            productId: 1,
            stock: 10);

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupProductResult(product);

        // Act
        Func<Task> act = async () =>
            await _cartService.AddToCartAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "İstenen ürün miktarı stok miktarını aşamaz.");

        _cartRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_CartDoesNotExist_CreatesCartAndCartItem()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 3
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 20);

        Cart? addedCart = null;
        CartItem? addedCartItem = null;

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupProductResult(product);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        _cartRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>(
                (cart, _) => addedCart = cart)
            .Returns(Task.CompletedTask);

        _cartItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()))
            .Callback<CartItem, CancellationToken>(
                (cartItem, _) =>
                    addedCartItem = cartItem)
            .Returns(Task.CompletedTask);

        // Act
        await _cartService.AddToCartAsync(dto);

        // Assert
        addedCart.Should().NotBeNull();
        addedCart!.CustomerId.Should()
            .Be(customer.CustomerId);

        addedCartItem.Should().NotBeNull();
        addedCartItem!.Cart.Should()
            .BeSameAs(addedCart);

        addedCartItem.ProductId.Should()
            .Be(dto.ProductId);

        addedCartItem.Quantity.Should()
            .Be(dto.Quantity);

        _cartRepositoryMock.Verify(
            x => x.AddAsync(
                addedCart,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _cartItemRepositoryMock.Verify(
            x => x.AddAsync(
                addedCartItem,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_CartExistsAndProductIsNotInCart_AddsNewCartItem()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 2,
            Quantity = 4
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 15);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        CartItem? addedCartItem = null;

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupProductResult(product);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        _cartItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()))
            .Callback<CartItem, CancellationToken>(
                (cartItem, _) =>
                    addedCartItem = cartItem)
            .Returns(Task.CompletedTask);

        // Act
        await _cartService.AddToCartAsync(dto);

        // Assert
        addedCartItem.Should().NotBeNull();
        addedCartItem!.CartId.Should()
            .Be(cart.CartId);

        addedCartItem.ProductId.Should()
            .Be(dto.ProductId);

        addedCartItem.Quantity.Should()
            .Be(dto.Quantity);

        _cartRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _cartItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ProductAlreadyInCart_IncreasesQuantity()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 3
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 10);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = product.ProductId,
            Quantity = 4
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupProductResult(product);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        // Act
        await _cartService.AddToCartAsync(dto);

        // Assert
        cartItem.Quantity.Should().Be(7);

        _cartItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_TotalCartQuantityExceedsStock_ThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 4
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 10);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = product.ProductId,
            Quantity = 7
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupProductResult(product);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        // Act
        Func<Task> act = async () =>
            await _cartService.AddToCartAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Sepetteki toplam ürün miktarı stok miktarını aşamaz.");

        cartItem.Quantity.Should().Be(7);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task CheckoutAsync_AddressDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 99
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Adres bulunamadı veya bu kullanıcıya ait değil.");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_CartDoesNotExist_RollsBackAndThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Cart>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Sepet boş.");

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_CartHasNoItems_RollsBackAndThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId,
            CartItems = new List<CartItem>()
        };

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        SetupCheckoutCart(cart);

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Sepet boş.");

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _orderRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ProductStockIsInsufficient_RollsBackAndThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        var product = CreateProduct(
            productId: 1,
            stock: 2,
            name: "Klavye");

        var cart = CreateCartWithItem(
            cartId: 20,
            customerId: customer.CustomerId,
            product: product,
            quantity: 3);

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        SetupCheckoutCart(cart);

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Klavye ürünü için yeterli stok bulunmamaktadır.");

        product.Stock.Should().Be(2);

        _orderRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_CreatesOrderDetailsUpdatesStockAndCommits()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        var firstProduct = CreateProduct(
            productId: 1,
            stock: 10,
            price: 750m,
            name: "Klavye");

        var secondProduct = CreateProduct(
            productId: 2,
            stock: 8,
            price: 500m,
            name: "Mouse");

        var firstCartItem = new CartItem
        {
            CartItemId = 1,
            CartId = 20,
            ProductId = firstProduct.ProductId,
            Product = firstProduct,
            Quantity = 2
        };

        var secondCartItem = new CartItem
        {
            CartItemId = 2,
            CartId = 20,
            ProductId = secondProduct.ProductId,
            Product = secondProduct,
            Quantity = 3
        };

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId,
            CartItems = new List<CartItem>
            {
                firstCartItem,
                secondCartItem
            }
        };

        Order? addedOrder = null;

        var addedOrderDetails =
            new List<OrderDetail>();

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        SetupCheckoutCart(cart);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>(
                (order, _) => addedOrder = order)
            .Returns(Task.CompletedTask);

        _orderDetailRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<OrderDetail>(),
                It.IsAny<CancellationToken>()))
            .Callback<OrderDetail, CancellationToken>(
                (detail, _) =>
                    addedOrderDetails.Add(detail))
            .Returns(Task.CompletedTask);

        var beforeCheckout = DateTime.UtcNow;

        // Act
        await _cartService.CheckoutAsync(dto);

        var afterCheckout = DateTime.UtcNow;

        // Assert
        addedOrder.Should().NotBeNull();

        addedOrder!.CustomerId.Should()
            .Be(customer.CustomerId);

        addedOrder.Status.Should()
            .Be(OrderStatus.Pending);

        addedOrder.TotalAmount.Should()
            .Be(3000m);

        addedOrder.ShippingAddressTitle.Should()
            .Be(address.Title);

        addedOrder.ShippingCity.Should()
            .Be(address.City);

        addedOrder.ShippingDistrict.Should()
            .Be(address.District);

        addedOrder.ShippingFullAddress.Should()
            .Be(address.FullAddress);

        addedOrder.OrderDate.Should()
            .BeOnOrAfter(beforeCheckout);

        addedOrder.OrderDate.Should()
            .BeOnOrBefore(afterCheckout);

        addedOrderDetails.Should().HaveCount(2);

        addedOrderDetails[0].Order.Should()
            .BeSameAs(addedOrder);

        addedOrderDetails[0].ProductId.Should()
            .Be(firstProduct.ProductId);

        addedOrderDetails[0].ProductName.Should()
            .Be(firstProduct.Name);

        addedOrderDetails[0].Quantity.Should()
            .Be(2);

        addedOrderDetails[0].UnitPrice.Should()
            .Be(750m);

        addedOrderDetails[1].Order.Should()
            .BeSameAs(addedOrder);

        addedOrderDetails[1].ProductId.Should()
            .Be(secondProduct.ProductId);

        addedOrderDetails[1].ProductName.Should()
            .Be(secondProduct.Name);

        addedOrderDetails[1].Quantity.Should()
            .Be(3);

        addedOrderDetails[1].UnitPrice.Should()
            .Be(500m);

        firstProduct.Stock.Should().Be(8);
        secondProduct.Stock.Should().Be(5);

        _productRepositoryMock.Verify(
            x => x.Update(firstProduct),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.Update(secondProduct),
            Times.Once);

        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(firstCartItem),
            Times.Once);

        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(secondCartItem),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_SaveChangesThrowsConcurrencyException_RollsBackAndThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        var product = CreateProduct(
            productId: 1,
            stock: 10,
            name: "Klavye");

        var cart = CreateCartWithItem(
            cartId: 20,
            customerId: customer.CustomerId,
            product: product,
            quantity: 2);

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        SetupCheckoutCart(cart);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DbUpdateConcurrencyException());

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen sepetinizi yenileyip tekrar deneyin.");

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_UnexpectedException_RollsBackAndRethrowsException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new CheckoutDto
        {
            AddressId = 5
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var address = CreateAddress(
            addressId: dto.AddressId,
            customerId: customer.CustomerId);

        var product = CreateProduct(
            productId: 1,
            stock: 10);

        var cart = CreateCartWithItem(
            cartId: 20,
            customerId: customer.CustomerId,
            product: product,
            quantity: 2);

        var expectedException =
            new InvalidOperationException(
                "Beklenmeyen veritabanı hatası.");

        SetupCheckoutCustomerAndAddress(
            appUserId,
            customer,
            address);

        SetupCheckoutCart(cart);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () =>
            await _cartService.CheckoutAsync(dto);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>();

        exception.Which.Should()
            .BeSameAs(expectedException);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(
                CancellationToken.None),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.CommitTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task GetMyCartAsync_CartDoesNotExist_ReturnsEmptyCartDto()
    {
        // Arrange
        const int appUserId = 7;

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Cart>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        var result =
            await _cartService.GetMyCartAsync();

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().Be(0);
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItemCount.Should().Be(0);
        result.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task GetMyCartAsync_CartExists_ReturnsCalculatedCartDto()
    {
        // Arrange
        const int appUserId = 7;

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var firstProduct = CreateProduct(
            productId: 1,
            stock: 10,
            price: 750m,
            name: "Klavye");

        var secondProduct = CreateProduct(
            productId: 2,
            stock: 10,
            price: 500m,
            name: "Mouse");

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId,
            CartItems = new List<CartItem>
            {
                new()
                {
                    CartItemId = 1,
                    CartId = 20,
                    ProductId = firstProduct.ProductId,
                    Product = firstProduct,
                    Quantity = 2
                },
                new()
                {
                    CartItemId = 2,
                    CartId = 20,
                    ProductId = secondProduct.ProductId,
                    Product = secondProduct,
                    Quantity = 3
                }
            }
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        SetupCheckoutCart(cart);

        // Act
        var result =
            await _cartService.GetMyCartAsync();

        // Assert
        result.CartId.Should().Be(cart.CartId);
        result.Items.Should().HaveCount(2);
        result.TotalItemCount.Should().Be(5);
        result.TotalAmount.Should().Be(3000m);

        result.Items[0].ProductId.Should()
            .Be(firstProduct.ProductId);

        result.Items[0].ProductName.Should()
            .Be("Klavye");

        result.Items[0].UnitPrice.Should()
            .Be(750m);

        result.Items[0].Quantity.Should()
            .Be(2);

        result.Items[0].LineTotal.Should()
            .Be(1500m);

        result.Items[1].ProductId.Should()
            .Be(secondProduct.ProductId);

        result.Items[1].ProductName.Should()
            .Be("Mouse");

        result.Items[1].UnitPrice.Should()
            .Be(500m);

        result.Items[1].Quantity.Should()
            .Be(3);

        result.Items[1].LineTotal.Should()
            .Be(1500m);
    }

    [Fact]
    public async Task RemoveCartItemAsync_CartDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.RemoveCartItemAsync(1);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sepet bulunamadı.");

        _cartItemRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveCartItemAsync_CartItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.RemoveCartItemAsync(50);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Ürün sepette bulunamadı.");

        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(
                It.IsAny<CartItem>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveCartItemAsync_CartItemExists_SoftDeletesAndSavesChanges()
    {
        // Arrange
        const int appUserId = 7;
        const int productId = 1;

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = productId,
            Quantity = 2
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        // Act
        await _cartService
            .RemoveCartItemAsync(productId);

        // Assert
        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(cartItem),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_QuantityIsNegative_ThrowsBadRequestException()
    {
        // Arrange
        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = -1
        };

        // Act
        Func<Task> act = async () =>
            await _cartService.UpdateCartItemAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Ürün miktarı negatif olamaz.");

        _currentUserServiceMock.Verify(
            x => x.GetAppUserId(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 2
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.UpdateCartItemAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Sepet bulunamadı.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 2
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.UpdateCartItemAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage(
                "Ürün sepette bulunamadı.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCartItemAsync_QuantityIsZero_SoftDeletesCartItem()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 0
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = dto.ProductId,
            Quantity = 3
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        // Act
        await _cartService.UpdateCartItemAsync(dto);

        // Assert
        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(cartItem),
            Times.Once);

        _productRepositoryMock.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 3
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = dto.ProductId,
            Quantity = 1
        };

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        _productRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () =>
            await _cartService.UpdateCartItemAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Ürün bulunamadı.");

        cartItem.Quantity.Should().Be(1);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCartItemAsync_QuantityExceedsStock_ThrowsBadRequestException()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 11
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = dto.ProductId,
            Quantity = 2
        };

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 10);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        SetupProductResult(product);

        // Act
        Func<Task> act = async () =>
            await _cartService.UpdateCartItemAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage(
                "İstenen ürün miktarı stok miktarını aşamaz.");

        cartItem.Quantity.Should().Be(2);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCartItemAsync_ValidQuantity_UpdatesQuantityAndSavesChanges()
    {
        // Arrange
        const int appUserId = 7;

        var dto = new UpdateCartItemDto
        {
            ProductId = 1,
            Quantity = 6
        };

        var customer = CreateCustomer(
            customerId: 10,
            appUserId: appUserId);

        var cart = new Cart
        {
            CartId = 20,
            CustomerId = customer.CustomerId
        };

        var cartItem = new CartItem
        {
            CartItemId = 30,
            CartId = cart.CartId,
            ProductId = dto.ProductId,
            Quantity = 2
        };

        var product = CreateProduct(
            productId: dto.ProductId,
            stock: 10);

        SetupCurrentCustomer(
            appUserId,
            customer);

        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Cart, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartItemRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<CartItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        SetupProductResult(product);

        // Act
        await _cartService.UpdateCartItemAsync(dto);

        // Assert
        cartItem.Quantity.Should().Be(dto.Quantity);

        _cartItemRepositoryMock.Verify(
            x => x.SoftDelete(
                It.IsAny<CartItem>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupCurrentCustomer(
        int appUserId,
        Customer customer)
    {
        _currentUserServiceMock
            .Setup(x => x.GetAppUserId())
            .Returns(appUserId);

        _customerServiceMock
            .Setup(x => x.GetByAppUserIdAsync(
                appUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
    }

    private void SetupProductResult(
        Product product)
    {
        _productRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
    }

    private void SetupCheckoutCustomerAndAddress(
        int appUserId,
        Customer customer,
        Address address)
    {
        SetupCurrentCustomer(
            appUserId,
            customer);

        _addressRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);
    }

    private void SetupCheckoutCart(
        Cart cart)
    {
        _cartRepositoryMock
            .Setup(x => x.GetAsync(
                It.IsAny<ISpecification<Cart>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
    }

    private static Customer CreateCustomer(
        int customerId,
        int appUserId)
    {
        return new Customer
        {
            CustomerId = customerId,
            AppUserId = appUserId,
            FirstName = "İsmet",
            LastName = "Akpınar",
            Phone = "05000000000"
        };
    }

    private static Product CreateProduct(
        int productId,
        int stock,
        decimal price = 100m,
        string name = "Test Ürünü")
    {
        return new Product
        {
            ProductId = productId,
            Name = name,
            Price = price,
            Stock = stock
        };
    }

    private static Address CreateAddress(
        int addressId,
        int customerId)
    {
        return new Address
        {
            AddressId = addressId,
            CustomerId = customerId,
            Title = "Ev",
            City = "İzmir",
            District = "Buca",
            FullAddress = "Örnek Mahallesi No: 10"
        };
    }

    private static Cart CreateCartWithItem(
        int cartId,
        int customerId,
        Product product,
        int quantity)
    {
        return new Cart
        {
            CartId = cartId,
            CustomerId = customerId,
            CartItems = new List<CartItem>
            {
                new()
                {
                    CartItemId = 1,
                    CartId = cartId,
                    ProductId = product.ProductId,
                    Product = product,
                    Quantity = quantity
                }
            }
        };
    }
}
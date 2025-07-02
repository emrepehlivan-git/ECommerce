global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Linq.Expressions;

// Testing Frameworks
global using Xunit;
global using FluentAssertions;
global using Moq;

// Core Dependencies
global using Ardalis.Result;

// Domain Layer
global using ECommerce.Domain.Entities;

// Application Layer
global using ECommerce.Application.Parameters;

global using ECommerce.Application.Features.Carts;
global using ECommerce.Application.Features.Categories;
global using ECommerce.Application.Features.Orders;
global using ECommerce.Application.Features.Products;
global using ECommerce.Application.Features.Roles;
global using ECommerce.Application.Features.UserAddresses;
global using ECommerce.Application.Features.Users;

global using ECommerce.Application.Features.Carts.V1.Commands;
global using ECommerce.Application.Features.Carts.V1.Queries;
global using ECommerce.Application.Features.Carts.V1.DTOs;
global using ECommerce.Application.Features.Categories.V1.Commands;
global using ECommerce.Application.Features.Categories.V1.Queries;
global using ECommerce.Application.Features.Categories.V1.DTOs;
global using ECommerce.Application.Features.Orders.V1.Commands;
global using ECommerce.Application.Features.Orders.V1.Queries;
global using ECommerce.Application.Features.Orders.V1.DTOs;
global using ECommerce.Application.Features.Products.V1.Commands;
global using ECommerce.Application.Features.Products.V1.Queries;
global using ECommerce.Application.Features.Products.V1.DTOs;
global using ECommerce.Application.Features.Roles.V1.Commands;
global using ECommerce.Application.Features.Roles.V1.Queries;
global using ECommerce.Application.Features.Roles.V1.DTOs;
global using ECommerce.Application.Features.Stock.V1.Commands;
global using ECommerce.Application.Features.Stock.V1.Queries;
global using ECommerce.Application.Features.UserAddresses.V1.Commands;
global using ECommerce.Application.Features.UserAddresses.V1.Queries;
global using ECommerce.Application.Features.UserAddresses.V1.DTOs;
global using ECommerce.Application.Features.Users.V1.Commands;
global using ECommerce.Application.Features.Users.V1.Queries;
global using ECommerce.Application.Features.Users.V1.DTOs;
global using ECommerce.Application.Repositories;

// Shared Kernel
global using ECommerce.SharedKernel.DependencyInjection;
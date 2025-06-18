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
global using ECommerce.Application.Features.Products.Commands;
global using ECommerce.Application.Repositories;

// Shared Kernel
global using ECommerce.SharedKernel.DependencyInjection;
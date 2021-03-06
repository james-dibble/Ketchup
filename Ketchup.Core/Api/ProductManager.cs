﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductManager.cs" company="James Dibble">
//    Copyright 2012 James Dibble
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Ketchup.Api
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using JamesDibble.ApplicationFramework.Data.Persistence;

    using Model.Product;

    /// <summary>
    /// A class to perform actions upon <see cref="Product"/>s.
    /// </summary>
    public sealed class ProductManager : IProductManager
    {
        private readonly IPersistenceManager _persistence;

        /// <summary>
        /// Initialises a new instance of the <see cref="ProductManager"/> class.
        /// </summary>
        /// <param name="persistenceManager">A connection to a persistence source.</param>
        public ProductManager(IPersistenceManager persistenceManager)
        {
            this._persistence = persistenceManager;
        }

        /// <summary>
        /// Build a new <see cref="ProductCategory"/> and save it.
        /// </summary>
        /// <param name="name">The name of the new <see cref="ProductCategory"/>.</param>
        /// <param name="specification">
        /// The attributes required for <see cref="Product"/>s of this <see cref="ProductCategory"/>.
        /// </param>
        /// <returns>The new <see cref="ProductCategory"/>.</returns>
        public ProductCategory CreateProductCategory(string name, IEnumerable<ProductCategorySpecificationAttribute> specification)
        {
            var productCategory = new ProductCategory { Name = name, Specification = specification.ToList() };

            this._persistence.Add(productCategory);

            this._persistence.Commit();

            return productCategory;
        }

        /// <summary>
        /// Build a new <see cref="Product"/> and save it.
        /// </summary>
        /// <param name="productSpecification">The attributes of the new <see cref="Product"/>.</param>
        /// <param name="category">The parent <see cref="ProductCategory"/> of the new <see cref="Product"/>.</param>
        /// <returns>The new <see cref="Product"/>.</returns>
        public Product CreateProduct(ProductSpecification productSpecification, ProductCategory category)
        {
            var product = new Product
            {
                ProductSpecifications = new Collection<ProductSpecification> { productSpecification },
                Category = category
            };

            this._persistence.Add(product);

            this._persistence.Commit();

            return product;
        }

        /// <summary>
        /// Concatenate an updated <see cref="ProductSpecification"/> to a given <paramref name="product"/>.
        /// </summary>
        /// <param name="product">The <see cref="Product"/> to update.</param>
        /// <param name="updatedSpecification">The updated <see cref="ProductSpecification"/>.</param>
        /// <returns>The updated <paramref name="product"/>.</returns>
        public Product UpdateProduct(Product product, ProductSpecification updatedSpecification)
        {
            var productToUpdate = this.GetProduct(product.Id);

            productToUpdate.ProductSpecifications.Add(updatedSpecification);

            this._persistence.Commit();

            return productToUpdate;
        }

        /// <summary>
        /// Build a new <see cref="ProductAttributeType"/> and save it.
        /// </summary>
        /// <param name="name">The name of the new <see cref="ProductAttributeType"/>.</param>
        /// <param name="displayName">A human readable value of the <paramref name="name"/>.</param>
        /// <param name="validationRegularExpression">A regular expression to validate the value of a <see cref="ProductAttribute"/>.</param>
        /// <returns>The new <see cref="ProductAttributeType"/>.</returns>
        public ProductAttributeType CreateAttributeType(string name, string displayName, string validationRegularExpression)
        {
            if (
                this._persistence.Find(
                    new PersistenceSearcher<ProductAttributeType>(
                        pat => pat.Name.ToLower() == name.ToLower()))
                != null)
            {
                throw new InvalidOperationException(
                    string.Format("A Product Attribute type with the name [{0}] already exists.", name));
            }

            try
            {
                var match = Regex.Match(string.Empty, validationRegularExpression);
            }
            catch (ArgumentException invalidRegexException)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.CurrentCulture,
                    "The regular expression [{0}] to validate the product attribute type [{1}] is not a valid regular expression.",
                    validationRegularExpression,
                    name),
                    invalidRegexException);
            }

            var productAttributeType = new ProductAttributeType
                                       {
                                           DisplayName = displayName,
                                           Name = name,
                                           ValidationRegularExpression = validationRegularExpression
                                       };

            this._persistence.Add(productAttributeType);

            this._persistence.Commit();

            return productAttributeType;
        }

        /// <summary>
        /// Retrieve all known <see cref="ProductAttributeType"/>s.
        /// </summary>
        /// <returns>All known <see cref="ProductAttributeType"/>s.</returns>
        public IEnumerable<ProductAttributeType> GetProductAttributeTypes()
        {
            var attributeTypes = this._persistence.Find(new PersistenceCollectionSearcher<ProductAttributeType>());

            return attributeTypes;
        }

        /// <summary>
        /// Retrieve a <see cref="ProductAttributeType"/> by ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ProductAttributeType"/> required.</param>
        /// <returns>
        /// A <see cref="ProductAttributeType"/> or null if no single <see cref="ProductAttributeType"/> exists.
        /// </returns>
        public ProductAttributeType GetProductAttributeType(int id)
        {
            var attribute =
               this._persistence.Find(new PersistenceSearcher<ProductAttributeType>(pat => pat.Id == id));

            return attribute;
        }

        /// <summary>
        /// Retrieve a <see cref="ProductAttributeType"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="ProductAttributeType"/> required.</param>
        /// <returns>
        /// A <see cref="ProductAttributeType"/> or null if no single <see cref="ProductAttributeType"/> exists.
        /// </returns>
        public ProductAttributeType GetProductAttributeType(string name)
        {
            var attribute =
                this._persistence.Find(new PersistenceSearcher<ProductAttributeType>(pat => pat.Name.ToLower() == name.ToLower()));

            return attribute;
        }

        /// <summary>
        /// Retrieve all known <see cref="Product"/>s.
        /// </summary>
        /// <returns>All known <see cref="Product"/>s.</returns>
        public IEnumerable<Product> GetProducts()
        {
            var products = this._persistence.Find(new PersistenceCollectionSearcher<Product>());

            return products;
        }

        /// <summary>
        /// Retrieve all <see cref="Product"/>s that match a given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">An expression to match <see cref="Product"/>s by.</param>
        /// <returns>All <see cref="Product"/>s that match a given <paramref name="predicate"/>.</returns>
        public IEnumerable<Product> GetProducts(Func<Product, bool> predicate)
        {
            var products = this._persistence.Find(new PersistenceCollectionSearcher<Product>(predicate));

            return products;
        }

        /// <summary>
        /// Retrieve all <see cref="Product"/>s that have the same <see cref="ProductAttribute"/>(s).
        /// </summary>
        /// <param name="relatedProductAttributes">The <see cref="ProductAttribute"/>s to match upon.</param>
        /// <returns>All <see cref="Product"/>s that have the same <see cref="ProductAttribute"/>(s).</returns>
        public IEnumerable<Product> GetRelatedProducts(params ProductAttribute[] relatedProductAttributes)
        {
            var relatedProducts = this.GetRelatedProducts(relatedProductAttributes.AsEnumerable());

            return relatedProducts;
        }

        /// <summary>
        /// Retrieve all <see cref="Product"/>s that have the same <see cref="ProductAttribute"/>(s).
        /// </summary>
        /// <param name="category">The <see cref="ProductCategory"/> the matching <see cref="Product"/>s must belong.</param>
        /// <param name="relatedProductAttributes">The <see cref="ProductAttribute"/>s to match upon.</param>
        /// <returns>All <see cref="Product"/>s that have the same <see cref="ProductAttribute"/>(s).</returns>
        public IEnumerable<Product> GetRelatedProducts(ProductCategory category, params ProductAttribute[] relatedProductAttributes)
        {
            var relatedProducts = this.GetRelatedProducts(relatedProductAttributes.AsEnumerable());

            relatedProducts = relatedProducts.Where(rp => rp.Category.Id == category.Id);

            return relatedProducts;
        }

        /// <summary>
        /// Retrieve all known <see cref="ProductCategory"/>s.
        /// </summary>
        /// <returns>All known <see cref="ProductCategory"/>s.</returns>
        public IEnumerable<ProductCategory> GetProductCategories()
        {
            var categories = this._persistence.Find(new PersistenceCollectionSearcher<ProductCategory>());

            return categories;
        }

        /// <summary>
        /// Retrieve the child <see cref="ProductCategory"/>s for a given <paramref name="parentCategory"/>.
        /// </summary>
        /// <param name="parentCategory">The <see cref="ProductCategory"/> to get child categories for.</param>
        /// <returns>The child <see cref="ProductCategory"/>s</returns>
        public IEnumerable<ProductCategory> GetProductCategories(ProductCategory parentCategory)
        {
            var categories = this._persistence.Find(new PersistenceCollectionSearcher<ProductCategory>(
                pc => pc.ParentCategory == parentCategory));

            return categories;
        }

        /// <summary>
        /// Retrieve all the distinct <see cref="ProductAttribute"/>s for a <see cref="ProductCategory"/> and
        /// <see cref="ProductAttributeType"/> based on their value.
        /// </summary>
        /// <param name="category">The <see cref="ProductCategory"/> to find the unique attributes for.</param>
        /// <param name="attributeType">The <see cref="ProductAttributeType"/> to find the unique attributes for.</param>
        /// <returns>A collection of distinct <see cref="ProductAttribute"/>s.</returns>
        public IEnumerable<ProductAttribute> GetUniqueProductAttributes(ProductCategory category, ProductAttributeType attributeType)
        {
            var attibutes = this._persistence.Find(
                new PersistenceCollectionSearcher<ProductAttribute>(
                    pa => pa.AttributeType.Id == attributeType.Id &&
                          pa.ProductSpecification.Category.Id == category.Id))
                .DistinctBy(pa => pa.Value.ToLower());

            return attibutes;
        }

        /// <summary>
        /// Retrieve a <see cref="ProductCategory"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="ProductCategory"/> required.</param>
        /// <returns>
        /// A <see cref="ProductCategory"/> or null if no single <see cref="ProductCategory"/> exists.
        /// </returns>
        public ProductCategory GetProductCategory(string name)
        {
            var category =
                this._persistence.Find(new PersistenceSearcher<ProductCategory>(pat => pat.Name.ToLower() == name.ToLower()));

            return category;
        }

        /// <summary>
        /// Retrieve a <see cref="ProductCategory"/> by ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ProductCategory"/> required.</param>
        /// <returns>
        /// A <see cref="ProductCategory"/> or null if no single <see cref="ProductCategory"/> exists.
        /// </returns>
        public ProductCategory GetProductCategory(int id)
        {
            var category =
                this._persistence.Find(new PersistenceSearcher<ProductCategory>(pat => pat.Id == id));

            return category;
        }

        /// <summary>
        /// Retrieve a <see cref="Product"/> by ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ProductCategory"/> required.</param>
        /// <returns>
        /// A <see cref="Product"/> or null if no single <see cref="Product"/> exists.
        /// </returns>
        public Product GetProduct(int id)
        {
            var product = this._persistence.Find(
                new PersistenceSearcher<Product>(p => p.Id == id));

            return product;
        }

        /// <summary>
        /// Retrieve a <see cref="Product"/> that matches a given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">An expression to match <see cref="Product"/> by.</param>
        /// <returns>
        /// A <see cref="Product"/> or null if no single <see cref="Product"/> exists.
        /// </returns>
        public Product GetProduct(Func<Product, bool> predicate)
        {
            var product = this._persistence.Find(
                new PersistenceSearcher<Product>(predicate));

            return product;
        }

        private IEnumerable<Product> GetRelatedProducts(IEnumerable<ProductAttribute> relatedProductAttributes)
        {
            var relatedProducts =
                relatedProductAttributes.Aggregate(
                    this.GetProducts(), 
                        (current, additionalRelatedProductAttribute) =>
                            current.Where(p => p.ActiveSpecification.ProductAttributes.Any(
                                pa => pa.AttributeType.Id == additionalRelatedProductAttribute.AttributeType.Id
                                    && pa.Value.ToLower() == additionalRelatedProductAttribute.Value.ToLower())));

            return relatedProducts;
        }
    }
}
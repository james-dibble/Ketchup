﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KetchupInitialiser.cs" company="James Dibble">
//    Copyright 2012 James Dibble
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Ketchup.Persistence.EntityFramework
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    using Ketchup.Persistence.EntityFramework.Migrations;

    /// <summary>
    /// A class to setup the Ketchup Entity Framework context.
    /// </summary>
    public class KetchupInitialiser : IDbContextFactory<KetchupContext>
    {
        /// <summary>
        /// Build a <see cref="KetchupContext"/> upgrading the database as it goes.
        /// </summary>
        /// <typeparam name="TKetchupInitialiser">
        /// The <see cref="KetchupInitialiser"/> type to be used to build the context.
        /// </typeparam>
        /// <typeparam name="TConfiguration">
        /// The <see cref="KetchupContextConfiguration{TSeeder}"/> type to use to seed the database with.
        /// </typeparam>
        /// <typeparam name="TSeeder">
        /// The type to insert objects after migration with.
        /// </typeparam>
        /// <returns>
        /// A <see cref="KetchupContext"/> to inject into 
        /// the <see cref="JamesDibble.ApplicationFramework.Data.Persistence.IPersistenceManager"/>.
        /// </returns>
        public static KetchupContext Initialise<TKetchupInitialiser, TConfiguration, TSeeder>()
            where TKetchupInitialiser : KetchupInitialiser, new()
            where TConfiguration : KetchupContextConfiguration<TSeeder>, new()
            where TSeeder : IKetchupSeeder, new()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<KetchupContext, TConfiguration>());

            var context = new TKetchupInitialiser().Create();

            context.ProductCategorys.Any();

            return context;
        }

        /// <summary>
        /// Creates a new instance of a derived <see cref="T:System.Data.Entity.DbContext"/> type.
        /// </summary>
        /// <remarks>
        /// Override this method to change the way <see cref="KetchupContext"/>s are created.
        /// </remarks>
        /// <returns>
        /// An instance of TContext 
        /// </returns>
        public virtual KetchupContext Create()
        {
            return new KetchupContext("KetchupConnection");
        }
    }
}
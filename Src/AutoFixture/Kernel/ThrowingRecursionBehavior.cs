﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ploeh.AutoFixture.Kernel
{
    /// <summary>
    /// Decorates a <see cref="ISpecimenBuilder"/> with a <see cref="ThrowingRecursionGuard"/>.
    /// </summary>
    public class ThrowingRecursionBehavior : ISpecimenBuilderTransformation
    {
        #region ISpecimenBuilderTransformation Members

        /// <summary>
        /// Decorates the supplied <see cref="ISpecimenBuilder"/> with a
        /// <see cref="ThrowingRecursionGuard"/>.
        /// </summary>
        /// <param name="builder">The builder to decorate.</param>
        /// <returns>
        /// <paramref name="builder"/> decorated with a <see cref="NullRecursionGuard"/>.
        /// </returns>
        public ISpecimenBuilder Transform(ISpecimenBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return new ThrowingRecursionGuard(builder);
        }

        #endregion
    }
}
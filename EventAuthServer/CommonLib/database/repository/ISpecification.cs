﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace med.common.library.database.repository
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }

        List<string> IncludeStrings { get; }

        Expression<Func<T, object>> OrderBy { get; }

        Expression<Func<T, object>> OrderByDescending { get; }

        // TODO: Paging should be optimized
        int Take { get; }

        int Skip { get; }

        bool isPagingEnabled { get; }
    }
}

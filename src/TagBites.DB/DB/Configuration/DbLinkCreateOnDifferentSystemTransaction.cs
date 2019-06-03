using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.Configuration
{
    public enum DbLinkCreateOnDifferentSystemTransaction
    {
        /// <summary>
        /// Creates DbLink with DbContext assigned to current transaction if exists. Otherwise creates DbLink with new/free DbContext.
        /// </summary>
        CreateLinkWithNewContextOrAssigedToCurrentTransaction,

        /// <summary>
        /// Copy transaction to current scope if Transaction.Current is null. Otherwise throw exception.
        /// </summary>
        TryToMoveTransactionOrThrowException,

        /// <summary>
        /// Throw exception.
        /// </summary>
        ThrowException
    }
}
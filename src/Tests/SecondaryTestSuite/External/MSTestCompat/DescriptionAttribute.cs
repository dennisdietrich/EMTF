/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class DescriptionAttribute : Attribute
    {
        private String _description;

        public String Description
        {
            get
            {
                return _description;
            }
        }

        public DescriptionAttribute(String description)
        {
            _description = description;
        }
    }
}
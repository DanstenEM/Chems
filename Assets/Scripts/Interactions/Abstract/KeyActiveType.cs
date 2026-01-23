using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Interactions.Abstract
{
    public enum KeyActiveType
    {
        /// <summary>
        /// Клик
        /// </summary>
        Tap,

        /// <summary>
        /// Удержание
        /// </summary>
        Hold,

        /// <summary>
        /// несколько кликов (двойной клик)
        /// </summary>
        MultiTap
    }
}

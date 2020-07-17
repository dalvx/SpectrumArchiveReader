﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SpectrumArchiveReader
{
    public static class GP
    {
        public static NumberFormatInfo NumberFormatInfo = new NumberFormatInfo();

        static GP()
        {
            NumberFormatInfo.NumberGroupSeparator = " ";
        }

        /// <summary>
        /// Перевод Double в строку. decimals - число символов после точки. Нули в конце печатаются. 
        /// Разделения по группам не происходит. Строк вида 1E-9 и 1E+9 не бывает. Десятичный разделитель - точка.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string ToString(double d, int decimals)
        {
            return d.ToString("F" + decimals, NumberFormatInfo);
        }
    }
}

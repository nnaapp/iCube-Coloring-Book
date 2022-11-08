using System.Collections;
using System.Collections.Generic;
using System;

namespace BookLib
{
    public class ButtonTypeNotFoundException : Exception
    {
        public ButtonTypeNotFoundException()
        {
        }

        public ButtonTypeNotFoundException(string message) : base(message)
        {
        }

        public ButtonTypeNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class InvalidPopException : Exception
    {
        public InvalidPopException()
        {
        }

        public InvalidPopException(string message) : base(message)
        {
        }

        public InvalidPopException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

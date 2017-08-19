using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Game_Exceptions
{
    class NotReadyException : UnityException
    {
        public NotReadyException()
        {
        }

        public NotReadyException(string message) : base(message)
        {
        }
    }
}

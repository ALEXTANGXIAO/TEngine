using System;

namespace TEngine
{
    public class LruGroupAttribute:Attribute
    {
        public int Capacity;
        
        public LruGroupAttribute(int capacity)
        {
            Capacity = capacity;
        }
    }
}
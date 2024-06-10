using System;

namespace VinhLB
{
    public interface IClickable
    {
        event Action OnClicked;
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace HabitTracker
{
    internal class Habit
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int Quantity { get; set; }

        public bool IsCompleted {  get; set; }
    }
}

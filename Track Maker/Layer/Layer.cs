﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Track_Maker
{
    public class Layer
    {
        public List<Storm> AssociatedStorms { get; set; }
        public string Name { get; set; }
        public Guid LayerId { get; set; }
       
        public Layer()
        {
            AssociatedStorms = new List<Storm>();
            GenerateGUID();
        }

        private void GenerateGUID()
        {
            LayerId = Guid.NewGuid();
        }

        public void AddStorm(Storm Sto)
        {
            AssociatedStorms.Add(Sto);
        }

        public void RemoveStorm(Storm Sto)
        {
            AssociatedStorms.Remove(Sto); 
        }
    }
}

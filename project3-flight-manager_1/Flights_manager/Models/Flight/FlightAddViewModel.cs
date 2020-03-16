using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Flights_manager.Models.Flight
{
    public class FlightAddViewModel
    {
        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        [Required]
        public DateTime TakeOff 
        {
            get
            {
                return this.takeOff.HasValue
                ? this.takeOff.Value
                : DateTime.Now;
            }
            set
            { this.takeOff = value; }
        }
        private DateTime? takeOff = null;

        [Required]
        public DateTime Landing
        {
            get
            {
                return this.landing.HasValue
                ? this.landing.Value
                : DateTime.Now;
            }
            set
            { this.landing = value; }
        }
        private DateTime? landing = null;

        [Required]
        public string TypePlane { get; set; }

        [Required]
        public int PlaneId { get; set; }

        [Required]
        public string PilotName { get; set; }

        [Required]
        public int AvailablePassengerSeats { get; set; }

        [Required]
        public int AvailableBusinessClassSeats { get; set; }
    }
}

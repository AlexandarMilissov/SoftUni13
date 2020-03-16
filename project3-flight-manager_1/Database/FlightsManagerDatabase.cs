using Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class FlightsManagerDatabase : DbContext
    {
        /*public Flights_manager_DB(DbContextOptions<Flights_manager_DB> options)
            :base(options)
        { }
*/

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=FlightsManagerDB;Trusted_Connection=True;Integrated Security = True;");
        }
    }
}

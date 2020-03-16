using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Flights_manager.Models;
using Database;
using Database.Entities;
using Flights_manager.Models.Shared;
using Microsoft.EntityFrameworkCore;
using Flights_manager.Models.Flight;
using Flights_manager.Models.Passenger;
using Flights_manager.Models.Reservation;
using System.Net.Mail;
using System.Net;

namespace Flights_manager.Controllers
{
    public class MyController : Controller
    {
        private readonly ILogger<MyController> _logger;
        private readonly FlightsManagerDatabase _context;
        private int NumberOfEmployeesToShow = 50;
        private int reservationsCount;
        static Employee LoggedIn;
        static int currFlightID = -1;
        static int currReservationID = -1;

        public MyController(ILogger<MyController> logger)
        {
            _logger = logger;
            _context = new FlightsManagerDatabase(/*DbContextOptionsBuilder.options.UseSqlServer(connection)*/);
            reservationsCount = _context.Reservations.Count();
        }

        public IActionResult Index()
        {
            return View();
        }

         public async Task<IActionResult> ManageFlights()
         {
            if(LoggedIn != null )
            {
                return RedirectToAction(nameof(ListFlights));
            }
            return RedirectToAction(nameof(Index));
         }
        [HttpGet]
        public async Task<IActionResult> Index(FlightListViewModel model)
        {
            List<SingleFlightViewModel> items = await _context.Flights.Select(f => new SingleFlightViewModel()
            {
                Id = f.Id,
                From = f.From,
                To = f.To,
                TakeOff = f.TakeOff,
                Landing = f.Landing,
                TypePlane = f.TypePlane,
                PlaneId = f.PlaneId,
                PilotName = f.PilotName,
                AvailablePassengerSeats = f.AvailablePassengerSeats,
                AvailableBusinessClassSeats = f.AvailableBusinessClassSeats
            }).ToListAsync();
            model.Flights = items;
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Size10(EmployeeListViewModel model)
        {
            NumberOfEmployeesToShow = 10;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(EmployeeLoginViewModel model)
        {
            if (_context.Employees.Any(e => e.Username == model.Username))
            {
                if (_context.Employees.Any(e => e.Password == model.Password))
                {

                    LoggedIn = _context.Employees.FirstOrDefault(e => e.Username == model.Username);
                    if (_context.Employees.Any(e => e.Username == model.Username && e.Role == "Administrator"))
                    {
                        return RedirectToAction(nameof(ListEmployees)); 
                    }
                    else if (_context.Employees.Any(e => e.Username == model.Username && e.Role == "Employee"))
                    {
                        return RedirectToAction(nameof(ListFlights));
                    }

                }
            }
            return View();
        }

        public async Task<IActionResult> Register(EmployeeRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = new Employee();
                if (_context.Employees.Count() == 0)
                {
                    employee.Role = "Administrator";
                }
                else
                {
                    employee.Role = "Employee";
                }
                employee.Username = model.Username;
                employee.Password = model.Password;
                employee.Email = model.Email;
                employee.Firstname = model.Firstname;
                employee.Lastname = model.Lastname;
                employee.EGN = model.EGN;
                employee.Address = model.Address;
                employee.PhoneNumber = model.PhoneNumber;
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListEmployees));
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ListEmployees(EmployeeListViewModel model)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            List<SingleEmployeeViewModel> items = await _context.Employees.Skip((model.Pager.CurrentPage - 1) * NumberOfEmployeesToShow).Take(NumberOfEmployeesToShow).Select(e => new SingleEmployeeViewModel()
            {
                Id = e.Id,
                Username = e.Username,
                Password = e.Password,
                Email = e.Email,
                Firstname = e.Firstname,
                Lastname = e.Lastname,
                EGN = e.EGN,
                Address = e.Address,
                PhoneNumber = e.PhoneNumber,
                Role = e.Role
            }).ToListAsync();
            model.RegisteredEmployees = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Employees.CountAsync() / (double)NumberOfEmployeesToShow);

            return View(model);
        }

        public async Task<IActionResult> EditEmployee(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            EmployeeEditViewModel model = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Username = employee.Username,
                Password = employee.Password,
                Email = employee.Email,
                Firstname = employee.Firstname,
                Lastname = employee.Lastname,
                EGN = employee.EGN,
                Address = employee.Address,
                PhoneNumber = employee.PhoneNumber,
                Role = employee.Role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = new Employee
                {
                    Id = model.Id,
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email,
                    Firstname = model.Firstname,
                    Lastname = model.Lastname,
                    EGN = model.EGN,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role
                };

                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(ListEmployees));
            }

            return View(model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            Employee employee = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListEmployees));
        }
        public async Task<IActionResult> AddFlight(FlightAddViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Landing < model.TakeOff)
                {
                    return View(model);
                }
                Flight flight = new Flight();
                flight.From = model.From;
                flight.To = model.To;
                flight.TakeOff = model.TakeOff;
                flight.Landing = model.Landing;
                flight.TypePlane = model.TypePlane;
                flight.PlaneId = model.PlaneId;
                flight.PilotName = model.PilotName;
                flight.AvailablePassengerSeats = model.AvailablePassengerSeats;
                flight.AvailableBusinessClassSeats = model.AvailableBusinessClassSeats;
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListFlights));
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ListFlights(FlightListViewModel model)
        {
            List<SingleFlightViewModel> items = await _context.Flights.Select(f => new SingleFlightViewModel()
            {
                Id = f.Id,
                From = f.From,
                To = f.To,
                TakeOff = f.TakeOff,
                Landing = f.Landing,
                TypePlane = f.TypePlane,
                PlaneId = f.PlaneId,
                PilotName = f.PilotName,
                AvailablePassengerSeats = f.AvailablePassengerSeats,
                AvailableBusinessClassSeats = f.AvailableBusinessClassSeats
            }).ToListAsync();
            model.Flights = items;
            return View(model);
        }
        public async Task<IActionResult> EditFlight(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Flight flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            FlightEditViewModel model = new FlightEditViewModel
            {
                Id = flight.Id,
                From = flight.From,
                To = flight.To,
                TakeOff = flight.TakeOff,
                Landing = flight.Landing,
                TypePlane = flight.TypePlane,
                PlaneId = flight.PlaneId,
                PilotName = flight.PilotName,
                AvailablePassengerSeats = flight.AvailablePassengerSeats,
                AvailableBusinessClassSeats = flight.AvailableBusinessClassSeats
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFlight(FlightEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Flight flight = new Flight
                {
                    Id = model.Id,
                    From = model.From,
                    To = model.To,
                    TakeOff = model.TakeOff,
                    Landing = model.Landing,
                    TypePlane = model.TypePlane,
                    PlaneId = model.PlaneId,
                    PilotName = model.PilotName,
                    AvailablePassengerSeats = model.AvailablePassengerSeats,
                    AvailableBusinessClassSeats = model.AvailableBusinessClassSeats
                };

                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(flight.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(ListFlights));
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteFlight(int id)
        {
            Flight flight = await _context.Flights.FindAsync(id);
            var reservations = _context.Reservations.Where(x => x.FlightId == id);
            var passangers = _context.Passengers.Where(x => reservations.FirstOrDefault(y => y.Id == x.ReservationId) != null);
            _context.Flights.Remove(flight);
            _context.Reservations.RemoveRange(reservations);
            _context.Passengers.RemoveRange(passangers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListFlights));
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPassenger(PassengerAddViewModel model)
        {
            if (ModelState.IsValid)
            {
                Passenger passenger = new Passenger
                {
                    FirstName = model.FirstName,
                    Surname = model.Surname,
                    Lastname = model.LastName,
                    EGN = model.EGN,
                    PhoneNumber = model.PhoneNumber,
                    Nationality = model.Nationality,
                    TicketType = model.TicketType,
                    ReservationId = currReservationID
                };
                Flight currFlight = _context.Flights.Find(currFlightID);

                if (passenger.TicketType == "Passenger"
                    && currFlight.AvailablePassengerSeats > 0)
                {
                    currFlight.AvailablePassengerSeats--;
                }
                else if (passenger.TicketType == "Business Class"
                      && currFlight.AvailableBusinessClassSeats > 0)
                {
                    currFlight.AvailableBusinessClassSeats--;
                }
                else
                {
                    return View(model);
                }

                _context.Add(passenger);
                await _context.SaveChangesAsync();
                SendEmail(_context.Reservations.Find(currReservationID).Email);

                return RedirectToAction(nameof(ListReservations));
            }
            return View(model);
        }
        void SendEmail(string to)
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("flightmanager666@gmail.com");
            mail.To.Add(to);
            mail.Subject = "Flight Reservation";
            mail.Body = "Reservation for the folloing people: \n";
            mail.BodyEncoding = System.Text.Encoding.UTF32;
            foreach(var p in _context.Passengers.Where(x => x.ReservationId == currReservationID))
            {
                mail.Body += p.FirstName + " " + p.Surname + " " + p.Lastname + "\n";
            }

            SmtpClient SmtpServer = new SmtpClient();
            SmtpServer.Host = "smtp.gmail.com";
            SmtpServer.Credentials = new System.Net.NetworkCredential("flightmanager666@gmail.com", "SoftUni13");
            SmtpServer.Port = 587;
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }

        public IActionResult AddPassenger()
        {
            PassengerAddViewModel passenger = new PassengerAddViewModel();
            return View(passenger);
        }
        public async Task<IActionResult> MakeReservation(ReservationMakeViewModel model,int id)
        {
            if(ModelState.IsValid)
            {
                Reservation reservation;
                currFlightID = id;
                try
                {
                    reservation = _context.Reservations.Single
                        (res => res.FlightId == id && res.Email == model.Email);
                }
                catch
                {
                    reservation = new Reservation()
                    {
                        FlightId = id,
                        Email = model.Email
                    };
                    _context.Add(reservation);
                    reservationsCount++;
                }

                await _context.SaveChangesAsync();
                currReservationID = reservation.Id;
                return RedirectToAction(nameof(AddPassenger));
            }
            return View(model);
        }

        public async Task<IActionResult> ListReservations(ReservationListViewModel model)
        {
            List<SingleReservationViewModel> items = await _context.Reservations.Select(r => new SingleReservationViewModel()
            {
                Id = r.Id,
                Email = r.Email
            }).ToListAsync();
            model.Reservations = items;
            return View(model);
        }
        private static List<SinglePassengerViewModel> PassengerConvertor(List<Passenger> passengers)
        {
            var converted = new List<SinglePassengerViewModel>();
            foreach(var item in passengers)
            {
                var model = new SinglePassengerViewModel
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    Surname = item.Surname,
                    LastName = item.Lastname,
                    EGN = item.EGN,
                    PhoneNumber = item.PhoneNumber,
                    Nationality = item.Nationality,
                    TicketType = item.TicketType,
                    ReservationId = item.ReservationId
                };
                converted.Add(model);
            }
            return converted;
        }
        public IActionResult ReservationDetails(int? id)
        {
            var model = new ReservationDetailsViewModel();
            model.Email = _context.Reservations.Find(id).Email;
            var passengers = new List<SinglePassengerViewModel>();
            foreach(var item in _context.Passengers.Where(p => p.ReservationId == id))
            {
                var passenger = new SinglePassengerViewModel
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    Surname = item.Surname,
                    LastName = item.Lastname,
                    EGN = item.EGN,
                    PhoneNumber = item.PhoneNumber,
                    Nationality = item.Nationality,
                    TicketType = item.TicketType,
                    ReservationId = item.ReservationId
                };
                passengers.Add(passenger);
            }
            model.Passengers = passengers;
            return View(model);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}

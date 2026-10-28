using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VoyagerTravel.Models;

namespace VoyagerTravel.Data.Seeding;

// Generates the same data on every run, relative to the day the database is created.
public static class DatabaseSeeder
{
    public const string Password = "Voyager-2026!";

    public static void Seed(DbContext context)
    {
        if (context.Set<Country>().Any())
        {
            return;
        }

        Populate(context);
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (await context.Set<Country>().AnyAsync(cancellationToken))
        {
            return;
        }

        Populate(context);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void Populate(DbContext context)
    {
        var data = new SeedData(new DateOnly(2026, 10, 18));
        data.Generate();

        context.AddRange(data.Roles);
        context.AddRange(data.Users);
        context.AddRange(data.UserRoles);
        context.AddRange(data.Countries);
        context.AddRange(data.Cities);
        context.AddRange(data.Destinations);
        context.AddRange(data.Tags);
        context.AddRange(data.Languages);
        context.AddRange(data.Amenities);
        context.AddRange(data.Guides);
        context.AddRange(data.Trips);
        context.AddRange(data.Suppliers);
        context.AddRange(data.Accommodations);
        context.AddRange(data.Excursions);
        context.AddRange(data.ItineraryDays);
        context.AddRange(data.Travellers);
        context.AddRange(data.Departures);
        context.AddRange(data.Bookings);
        context.AddRange(data.Passengers);
        context.AddRange(data.Payments);
        context.AddRange(data.AddOns);
        context.AddRange(data.Reviews);
        context.AddRange(data.RoomAllocations);
        context.AddRange(data.Invoices);
    }

    private sealed class SeedData(DateOnly today)
    {
        private static readonly (string Locale, string CountryCode, int Weight)[] TravellerLocales =
        [
            ("en_US", "US", 22), ("en_GB", "GB", 18), ("en_AU", "AU", 10), ("de", "DE", 10), ("en_CA", "CA", 8),
            ("nl", "NL", 6), ("fr", "FR", 6), ("en_IE", "IE", 3), ("sv", "SE", 3), ("es", "ES", 3), ("it", "IT", 3),
            ("pt_BR", "BR", 2), ("en_ZA", "ZA", 2), ("de_CH", "CH", 2), ("nb_NO", "NO", 2),
        ];

        private static readonly (string Email, string DisplayName, string Role)[] StaffAccounts =
        [
            ("admin@voyager.travel", "Alex Morgan", "Admin"),
            ("priya@voyager.travel", "Priya Nair", "Agent"),
            ("tomas@voyager.travel", "Tomás Ortega", "Agent"),
            ("hannah@voyager.travel", "Hannah Becker", "Agent"),
            ("kenji@voyager.travel", "Kenji Watanabe", "Agent"),
            ("chloe@voyager.travel", "Chloé Martin", "Agent"),
            ("liam@voyager.travel", "Liam O'Connor", "Agent"),
            ("viewer@voyager.travel", "Jordan Lee", "Viewer"),
        ];

        private static readonly string[] SpecialRequests =
        [
            "Vegetarian meals, please.", "We are celebrating our anniversary.", "Ground-floor room if possible.",
            "Gluten-free diet.", "One traveller uses a wheelchair.", "Early check-in on the first night, please.",
            "We'd like an extra night before the trip starts.", "Vegan meals for both travellers.", "Travelling with a teenager.",
            "Nut allergy — please inform the restaurants.", "Twin beds rather than a double.", "Arriving on a late flight.",
        ];

        private static readonly Dictionary<int, (string Title, string Body)[]> ReviewTexts = new()
        {
            [5] =
            [
                ("Trip of a lifetime", "Every day was better than the last. Our guide went above and beyond and the group was fantastic."),
                ("Absolutely perfect", "Flawless organisation, wonderful hotels and experiences we could never have arranged ourselves."),
                ("Exceeded every expectation", "I was nervous about travelling solo but felt looked after from start to finish."),
                ("Already planning the next one", "The perfect balance of guided activities and free time. Highly recommended."),
            ],
            [4] =
            [
                ("Wonderful, with a few long days", "Brilliant guide and amazing sights. Some of the driving days were long, but worth it."),
                ("Great value", "Really well run trip. One hotel was a little tired but everything else was excellent."),
                ("Loved it", "Fantastic itinerary. I would have liked a bit more free time in the cities."),
            ],
            [3] =
            [
                ("Good but rushed", "We saw a lot, but the pace meant we never really settled anywhere."),
                ("Mixed feelings", "Highlights were superb, but the group was larger than we expected."),
            ],
            [2] =
            [
                ("Not what we expected", "Several activities were changed at short notice and the communication was poor."),
            ],
            [1] =
            [
                ("Disappointing", "Our accommodation was cancelled on arrival and the replacement was far from the centre."),
            ],
        };

        private static readonly string[] SourceAirports = ["LHR", "MAN", "DUB", "JFK", "LAX", "YYZ", "SYD", "FRA", "AMS"];

        private static readonly string[] BrandColors = ["#0e7490", "#b45309", "#15803d", "#7c3aed", "#be123c", "#1d4ed8", "#a16207", "#0f766e"];

        private readonly Faker faker = CreateFaker("en");

        // Draws the values of properties added after the first release, so the existing data stays the same.
        private readonly Faker extras = new("en") { Random = new Randomizer(20261029) };
        private readonly Dictionary<string, Faker> localeFakers = [];
        private readonly HashSet<string> emails = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Destination> destinationsBySlug = [];
        private readonly Dictionary<Trip, (TripSeed Seed, List<(Accommodation Accommodation, int Nights)> Stays)> tripDetails = [];
        private int logoCount;

        public List<IdentityRole> Roles { get; } = [];
        public List<ApplicationUser> Users { get; } = [];
        public List<IdentityUserRole<string>> UserRoles { get; } = [];
        public List<Country> Countries { get; } = [];
        public List<City> Cities { get; } = [];
        public List<Destination> Destinations { get; } = [];
        public List<TripTag> Tags { get; } = [];
        public List<Language> Languages { get; } = [];
        public List<Amenity> Amenities { get; } = [];
        public List<Guide> Guides { get; } = [];
        public List<Trip> Trips { get; } = [];
        public List<Supplier> Suppliers { get; } = [];
        public List<Accommodation> Accommodations { get; } = [];
        public List<Excursion> Excursions { get; } = [];
        public List<ItineraryDay> ItineraryDays { get; } = [];
        public List<Traveller> Travellers { get; } = [];
        public List<Departure> Departures { get; } = [];
        public List<Booking> Bookings { get; } = [];
        public List<BookingPassenger> Passengers { get; } = [];
        public List<Payment> Payments { get; } = [];
        public List<BookingAddOn> AddOns { get; } = [];
        public List<Review> Reviews { get; } = [];
        public List<RoomAllocation> RoomAllocations { get; } = [];
        public List<SupplierInvoice> Invoices { get; } = [];

        private DateTime Now => today.ToDateTime(new TimeOnly(9, 0));

        public void Generate()
        {
            GenerateStaff();
            GenerateReferenceData();
            GenerateGuides();
            GenerateSuppliers();
            GenerateTrips();
            GenerateTravellers();
            GenerateDepartures();
            GenerateBookings();
            GenerateReviews();
            GenerateRoomAllocations();
            GenerateInvoices();
            GenerateBookingUpdates();
        }

        private static Faker CreateFaker(string locale) => new(locale) { Random = new Randomizer(20261018) };

        private Faker LocaleFaker(string locale)
        {
            if (!localeFakers.TryGetValue(locale, out var localeFaker))
            {
                localeFaker = new Faker(locale) { Random = new Randomizer(faker.Random.Int()) };
                localeFakers[locale] = localeFaker;
            }

            return localeFaker;
        }

        private void GenerateStaff()
        {
            var hasher = new PasswordHasher<ApplicationUser>();

            foreach (var name in new[] { "Admin", "Agent", "Viewer" })
            {
                Roles.Add(new IdentityRole(name) { Id = faker.Random.Guid().ToString(), NormalizedName = name.ToUpperInvariant(), ConcurrencyStamp = faker.Random.Guid().ToString() });
            }

            foreach (var (email, displayName, role) in StaffAccounts)
            {
                var user = new ApplicationUser
                {
                    Id = faker.Random.Guid().ToString(),
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    DisplayName = displayName,
                    SecurityStamp = faker.Random.Guid().ToString("N").ToUpperInvariant(),
                    ConcurrencyStamp = faker.Random.Guid().ToString(),
                    LockoutEnabled = true,
                };
                user.PasswordHash = hasher.HashPassword(user, Password);
                Users.Add(user);
                UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = Roles.Single(r => r.Name == role).Id });
            }
        }

        private void GenerateReferenceData()
        {
            foreach (var (isoCode, name, continent) in Catalog.Countries)
            {
                Countries.Add(new Country { IsoCode = isoCode, Name = name, Continent = continent });
            }

            foreach (var (countryCode, name, hasAirport) in Catalog.Cities)
            {
                Cities.Add(new City { Name = name, Country = Country(countryCode), HasAirport = hasAirport });
            }

            foreach (var seed in Catalog.Destinations)
            {
                var destination = new Destination
                {
                    Name = seed.Name,
                    Country = Country(seed.CountryCode),
                    ParentDestination = seed.ParentSlug is null ? null : destinationsBySlug[seed.ParentSlug],
                    Summary = seed.Summary,
                    HeroImageUrl = $"/images/destinations/{seed.Slug}.webp",
                    Climate = seed.Climate,
                    IsFeatured = seed.IsFeatured,
                    City = Cities.Single(city => city.Country.IsoCode == seed.CountryCode && city.Name == Catalog.DestinationCities[seed.Slug]),
                    RequiresAirport = Catalog.AirportDestinations.Contains(seed.Slug),
                };
                destinationsBySlug[seed.Slug] = destination;
                Destinations.Add(destination);
            }

            foreach (var destination in Destinations)
            {
                destination.Description = Descriptions.Destination(destination, Destinations.Where(child => child.ParentDestination == destination).ToList());
            }

            Tags.AddRange(Catalog.Tags.Select(name => new TripTag { Name = name }));
            Languages.AddRange(Catalog.Languages.Select(language => new Language { Code = language.Code, Name = language.Name }));
            Amenities.AddRange(Catalog.Amenities.Select(amenity => new Amenity { Name = amenity.Name, Icon = amenity.Icon }));
        }

        private void GenerateGuides()
        {
            var english = Languages.Single(language => language.Code == "en");
            var locales = new[] { "en_GB", "en_US", "es", "it", "fr", "de", "pt_PT", "en_AU", "en_ZA", "nl" };
            var homes = new[] { ("GB", "LHR"), ("US", "JFK"), ("ES", "MAD"), ("IT", "FCO"), ("FR", "CDG"), ("DE", "FRA"), ("PT", "LIS"), ("AU", "SYD"), ("ZA", "CPT"), ("NL", "AMS") };

            for (var i = 1; i <= 25; i++)
            {
                var localeFaker = LocaleFaker(locales[(i - 1) % locales.Length]);
                var firstName = localeFaker.Name.FirstName();
                var lastName = localeFaker.Name.LastName();
                var languages = new List<Language> { english };
                languages.AddRange(faker.PickRandom(Languages.Where(language => language != english), faker.Random.Int(1, 3)));
                var years = faker.Random.Int(1, 14);
                var (homeCountry, homeAirport) = homes[(i - 1) % homes.Length];

                Guides.Add(new Guide
                {
                    Id = i,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = UniqueEmail(faker.Internet.Email(firstName, lastName, "voyager.travel")),
                    Phone = Truncate(localeFaker.Phone.PhoneNumber(), 30),
                    PhotoUrl = $"/images/avatars/guides/guide-{i:00}.webp",
                    Bio = Descriptions.GuideBio(firstName, years, languages, faker.PickRandom(Descriptions.GuideStories), i),
                    HiredOn = today.AddDays(-faker.Random.Int(180, 365 * years + 180)),
                    IsActive = faker.Random.Bool(0.9f),
                    Languages = languages,
                    HomeAirport = homeAirport,
                    Country = Country(homeCountry),
                    BadgeCode = extras.Random.Replace("####"),
                });
            }
        }

        private void GenerateSuppliers()
        {
            var hotelImage = 0;

            foreach (var country in Countries.Where(country => Destinations.Any(destination => destination.Country == country)))
            {
                var leaves = Destinations.Where(destination => destination.Country == country && Destinations.All(child => child.ParentDestination != destination)).ToList();
                var hotelGroups = Enumerable.Range(0, Math.Clamp(leaves.Count, 1, 3))
                    .Select(_ => AddSupplier(country, SupplierType.Hotel, $"{faker.Name.LastName()} {faker.PickRandom("Hotels", "Hospitality", "Hotel Group", "Collection", "Stays")}"))
                    .ToList();
                var experiences = AddSupplier(country, SupplierType.Excursion, $"{faker.Name.LastName()} {faker.PickRandom("Experiences", "Tours", "Adventures", "Excursions")}");
                AddSupplier(country, SupplierType.Transport, $"{faker.Name.LastName()} {faker.PickRandom("Coaches", "Transfers", "Travel Services", "Logistics")}");

                if (faker.Random.Bool())
                {
                    AddSupplier(country, SupplierType.Restaurant, $"{faker.Name.LastName()} {faker.PickRandom("Dining", "Kitchen Collective", "Restaurant Group")}");
                }

                for (var leafIndex = 0; leafIndex < leaves.Count; leafIndex++)
                {
                    var destination = leaves[leafIndex];

                    for (var stay = 0; stay < 2; stay++)
                    {
                        var stars = faker.Random.WeightedRandom([3, 4, 5], [0.35f, 0.45f, 0.2f]);
                        var amenities = faker.PickRandom(Amenities, faker.Random.Int(4, 9)).ToList();
                        Accommodations.Add(new Accommodation
                        {
                            Supplier = hotelGroups[(leafIndex + stay) % hotelGroups.Count],
                            Destination = destination,
                            Name = faker.Random.Bool()
                                ? $"{destination.Name} {faker.PickRandom(Catalog.HotelNameSuffixes)}"
                                : $"The {faker.Name.LastName()} {faker.PickRandom(Catalog.HotelNameSuffixes)}",
                            StarRating = stars,
                            CheckInTime = faker.PickRandom(new TimeOnly(14, 0), new TimeOnly(15, 0), new TimeOnly(16, 0)),
                            CheckOutTime = faker.PickRandom(new TimeOnly(10, 0), new TimeOnly(11, 0), new TimeOnly(12, 0)),
                            ImageUrl = $"/images/hotels/hotel-{hotelImage++ % 24 + 1:00}.webp",
                            Amenities = amenities,
                        });
                    }

                    var templates = Catalog.ExcursionTemplates.Where(template => Suits(template.Kind, destination.Climate)).ToList();
                    foreach (var template in faker.PickRandom(templates, Math.Min(3, templates.Count)))
                    {
                        Excursions.Add(new Excursion
                        {
                            Supplier = experiences,
                            Destination = destination,
                            Name = $"{destination.Name} {template.Name}",
                            DurationHours = template.Hours,
                            Price = Math.Round(template.Price * faker.Random.Decimal(0.8m, 1.3m) / 5) * 5,
                            ImageUrl = $"/images/excursions/{template.Kind}-{faker.Random.Int(1, 2)}.webp",
                            IsActive = faker.Random.Bool(0.92f),
                        });
                    }
                }
            }
        }

        private static bool Suits(string kind, Climate climate) => kind switch
        {
            "balloon" => climate is Climate.Arid or Climate.Continental,
            "snorkel" => climate is Climate.Tropical or Climate.Mediterranean,
            "kayak" or "boat" => climate is not Climate.Arid,
            "wine" => climate is Climate.Mediterranean or Climate.Temperate or Climate.Continental,
            _ => true,
        };

        private Supplier AddSupplier(Country country, SupplierType type, string name)
        {
            var domain = new string(name.ToLowerInvariant().Where(char.IsAsciiLetterOrDigit).ToArray());
            var city = Destinations.First(destination => destination.Country == country).Name;
            var supplier = new Supplier
            {
                Name = name,
                Type = type,
                Country = country,
                ContactName = faker.Random.Bool(0.85f) ? faker.Name.FullName() : null,
                Email = UniqueEmail($"{faker.PickRandom("reservations", "partners", "hello", "bookings")}@{domain}.com"),
                Phone = faker.Random.Bool(0.9f) ? $"+{faker.Random.Int(20, 99)} {faker.Random.Int(100, 999)} {faker.Random.Int(1000, 9999)}" : null,
                Website = faker.Random.Bool(0.8f) ? $"https://www.{domain}.com" : null,
                LogoUrl = logoCount < 80 && type is SupplierType.Hotel or SupplierType.Transport ? $"/images/suppliers/supplier-{++logoCount:000}.webp" : null,
                Address = new Address
                {
                    Line1 = faker.Address.StreetAddress(),
                    Line2 = faker.Random.Bool(0.2f) ? faker.Address.SecondaryAddress() : null,
                    City = city,
                    PostalCode = faker.Random.Bool(0.85f) ? faker.Address.ZipCode() : null,
                    CountryCode = country.IsoCode,
                },
                ContractStatus = faker.Random.WeightedRandom([ContractStatus.Active, ContractStatus.Prospect, ContractStatus.Suspended], [0.8f, 0.12f, 0.08f]),
                CommissionRate = faker.Random.Int(16, 40) / 2m,
            };
            Suppliers.Add(supplier);
            return supplier;
        }

        private void GenerateTrips()
        {
            var sequenceByCountry = new Dictionary<string, int>();

            foreach (var seed in Catalog.Trips)
            {
                var destination = destinationsBySlug[seed.DestinationSlug];
                var countryCode = destination.Country.IsoCode;
                sequenceByCountry[countryCode] = sequenceByCountry.GetValueOrDefault(countryCode) + 1;

                var trip = new Trip
                {
                    Code = $"VT-{countryCode}-{sequenceByCountry[countryCode]:00}",
                    Title = seed.Title,
                    Destination = destination,
                    TripType = seed.TripType,
                    DurationDays = seed.DurationDays,
                    Difficulty = seed.Difficulty switch
                    {
                        1 => TripDifficulty.Easy,
                        2 or 3 => TripDifficulty.Moderate,
                        _ => TripDifficulty.Challenging,
                    },
                    BasePrice = seed.BasePrice,
                    MaxGroupSize = seed.TripType switch
                    {
                        TripType.Cruise => 24,
                        TripType.Wildlife => 10,
                        TripType.Adventure or TripType.Culinary => 12,
                        TripType.Wellness => 14,
                        _ => 16,
                    },
                    CoverImageUrl = $"/images/trips/{seed.DestinationSlug}.webp",
                    IsPublished = faker.Random.Bool(0.9f),
                    Tags = Tags.Where(tag => seed.Tags.Contains(tag.Name)).ToList(),
                    Guides = faker.PickRandom(Guides, faker.Random.Int(2, 3)).ToList(),
                    Summary = seed.Pitch,
                    Highlights = seed.Highlights.ToList(),
                    Transport = seed.TripType == TripType.Cruise ? "Ship" : seed.Tags.Contains("Train Journey") ? "Train" : "Plane",
                    Icon = seed.TripType switch
                    {
                        TripType.Adventure => "mountain",
                        TripType.Cultural => "landmark",
                        TripType.Wellness => "leaf",
                        TripType.Cruise => "ship",
                        TripType.CityBreak => "building",
                        TripType.Wildlife => "paw-print",
                        _ => "utensils",
                    },
                    BrandColor = extras.PickRandom(BrandColors),
                    DepartureAirports = extras.PickRandom(SourceAirports, extras.Random.Int(1, 3)).ToList(),
                    BaggageAllowance = seed.Tags.Contains("Luxury")
                        ? new() { ["Economy"] = 23, ["Business"] = 32, ["First"] = 40 }
                        : new() { ["Economy"] = seed.Tags.Contains("Budget") ? 20 : 23, ["Business"] = 32 },
                };
                trip.LeadGuide = extras.Random.Bool(0.85f) ? trip.Guides[0] : null;
                trip.GuidesByLanguage["en"] = trip.Guides[0].Id;
                if (trip.Guides[1].Languages.FirstOrDefault(language => language.Code != "en") is { } language)
                {
                    trip.GuidesByLanguage[language.Code] = trip.Guides[1].Id;
                }

                Trips.Add(trip);

                // The first stay is in the trip's destination; longer trips move on to a second one nearby.
                var nearby = Accommodations.Where(accommodation => accommodation.Destination.Country == destination.Country).ToList();
                var first = faker.PickRandom(nearby.Where(accommodation => accommodation.Destination == destination || accommodation.Destination.ParentDestination == destination).DefaultIfEmpty(nearby[0]));
                var nights = seed.DurationDays - 1;
                var stays = new List<(Accommodation Accommodation, int Nights)>();

                if (nights >= 7 && nearby.Count > 2)
                {
                    var second = faker.PickRandom(nearby.Where(accommodation => accommodation.Destination != first.Destination).DefaultIfEmpty(first));
                    stays.Add((first, nights / 2));
                    stays.Add((second, nights - nights / 2));
                }
                else if (nights > 0)
                {
                    stays.Add((first, nights));
                }

                tripDetails[trip] = (seed, stays);
                AddItinerary(trip, destination, stays);
                trip.Description = Descriptions.Trip(seed, trip, stays, ItineraryDays.Where(day => day.Trip == trip).ToList());
            }
        }

        private void AddItinerary(Trip trip, Destination destination, List<(Accommodation Accommodation, int Nights)> stays)
        {
            var nightAccommodation = stays.SelectMany(stay => Enumerable.Repeat(stay.Accommodation, stay.Nights)).ToList();
            var plans = faker.Random.Shuffle(Descriptions.ItineraryPlans[trip.TripType]).ToList();

            for (var day = 1; day <= trip.DurationDays; day++)
            {
                var accommodation = day <= nightAccommodation.Count ? nightAccommodation[day - 1] : null;
                var movingTo = accommodation is not null && day > 1 && accommodation != nightAccommodation[day - 2] ? accommodation : null;
                var (title, description) = day switch
                {
                    1 => ($"Arrival in {destination.Name}", Descriptions.ArrivalDay(destination, accommodation)),
                    _ when day == trip.DurationDays && day > 1 => ("Departure", Descriptions.DepartureDay()),
                    _ => (plans[(day - 2) % plans.Count].Title, Descriptions.ItineraryDay(plans[(day - 2) % plans.Count], faker.PickRandom(Descriptions.ItineraryTips), movingTo)),
                };
                var meals = accommodation is null ? Meals.None : Meals.Breakfast;
                meals |= day == 1 || faker.Random.Bool(0.3f) ? Meals.Dinner : Meals.None;
                meals |= faker.Random.Bool(0.25f) ? Meals.Lunch : Meals.None;

                ItineraryDays.Add(new ItineraryDay
                {
                    Trip = trip,
                    DayNumber = day,
                    Title = day > 1 && day < trip.DurationDays && day - 2 >= plans.Count ? $"{title} (Part {(day - 2) / plans.Count + 1})" : title,
                    Description = description,
                    Accommodation = accommodation,
                    Meals = meals,
                });
            }
        }

        private void GenerateTravellers()
        {
            var weights = TravellerLocales.Select(locale => (float)locale.Weight).ToArray();
            var locales = TravellerLocales.Select(locale => locale).ToArray();

            for (var i = 1; i <= 3000; i++)
            {
                var (locale, countryCode, _) = faker.Random.WeightedRandom(locales, weights);
                var localeFaker = LocaleFaker(locale);
                var firstName = localeFaker.Name.FirstName();
                var lastName = localeFaker.Name.LastName();

                Travellers.Add(new Traveller
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = UniqueEmail(localeFaker.Internet.Email(firstName, lastName)),
                    Phone = faker.Random.Bool(0.8f) ? Truncate(localeFaker.Phone.PhoneNumber(), 30) : null,
                    DateOfBirth = faker.Random.Bool(0.85f) ? today.AddDays(-faker.Random.Int(365 * 19, 365 * 78)) : null,
                    Nationality = faker.Random.Bool(0.95f) ? Country(countryCode) : null,
                    Address = new Address
                    {
                        Line1 = Truncate(localeFaker.Address.StreetAddress(), 120),
                        Line2 = faker.Random.Bool(0.15f) ? Truncate(localeFaker.Address.SecondaryAddress(), 120) : null,
                        City = Truncate(localeFaker.Address.City(), 80),
                        PostalCode = Truncate(localeFaker.Address.ZipCode(), 20),
                        CountryCode = countryCode,
                    },
                    MarketingOptIn = faker.Random.Bool(0.55f),
                    AvatarUrl = i <= 300 ? $"/images/avatars/travellers/traveller-{i:000}.webp" : null,
                    Notes = faker.Random.Bool(0.08f) ? faker.PickRandom("Prefers aisle seats.", "Repeat customer — always books with a friend.", "Asked for paper invoices.", "Photographer; interested in our photography trips.", "Prefers to be contacted by phone.") : null,
                    CreatedAt = Now.AddDays(-faker.Random.Int(30, 365 * 4)).AddMinutes(faker.Random.Int(0, 600)),
                });
            }
        }

        private void GenerateDepartures()
        {
            foreach (var trip in Trips)
            {
                var start = today.AddDays(-540 + faker.Random.Int(0, 40));

                while (start < today.AddDays(360))
                {
                    var past = start < today;
                    Departures.Add(new Departure
                    {
                        Trip = trip,
                        StartDate = start,
                        EndDate = start.AddDays(trip.DurationDays - 1),
                        MeetingTime = faker.Random.Bool(0.9f) ? faker.PickRandom(new TimeOnly(8, 0), new TimeOnly(9, 0), new TimeOnly(14, 0), new TimeOnly(16, 0), new TimeOnly(18, 0)) : null,
                        Capacity = trip.MaxGroupSize,
                        PriceOverride = faker.Random.Bool(0.15f) ? Math.Round(trip.BasePrice * faker.PickRandom(0.85m, 1.1m, 1.2m) / 10) * 10 : null,
                        LeadGuide = past || faker.Random.Bool(0.9f) ? faker.PickRandom(trip.Guides) : null,
                        Status = faker.Random.Bool(0.04f) ? DepartureStatus.Cancelled : DepartureStatus.Scheduled,
                    });
                    start = start.AddDays(faker.Random.Int(38, 52));
                }
            }
        }

        private void GenerateBookings()
        {
            var agents = Users.Where(user => UserRoles.Any(userRole => userRole.UserId == user.Id && userRole.RoleId == Roles.Single(role => role.Name == "Agent").Id)).ToList();
            var excursionsByDestination = Excursions.Where(excursion => excursion.IsActive).ToLookup(excursion => excursion.Destination);

            foreach (var departure in Departures)
            {
                var daysAway = departure.StartDate.DayNumber - today.DayNumber;
                var fill = daysAway switch
                {
                    < 0 => faker.Random.Double(0.5, 1),
                    <= 90 => faker.Random.Double(0.4, 1),
                    _ => faker.Random.Double(0, 0.6),
                };
                var target = (int)Math.Round(departure.Capacity * fill);
                var passengers = 0;

                while (passengers < target)
                {
                    var size = Math.Min(faker.Random.WeightedRandom([1, 2, 3, 4], [0.3f, 0.5f, 0.1f, 0.1f]), departure.Capacity - passengers);
                    var bookedAt = departure.StartDate.ToDateTime(new TimeOnly(0, 0)).AddDays(-faker.Random.Int(14, 270)).AddMinutes(faker.Random.Int(420, 1380));
                    if (bookedAt > Now)
                    {
                        bookedAt = Now.AddDays(-faker.Random.Int(0, 6)).AddMinutes(-faker.Random.Int(0, 400));
                    }

                    var status = departure.Status == DepartureStatus.Cancelled || faker.Random.Bool(daysAway < 0 ? 0.05f : 0.12f)
                        ? BookingStatus.Cancelled
                        : daysAway < 0 ? BookingStatus.Completed
                        : (Now - bookedAt).TotalDays < 21 && faker.Random.Bool(0.6f) ? BookingStatus.Pending
                        : BookingStatus.Confirmed;

                    AddBooking(departure, size, bookedAt, status, agents, excursionsByDestination);
                    if (status != BookingStatus.Cancelled)
                    {
                        passengers += size;
                    }
                    else if (departure.Status == DepartureStatus.Cancelled)
                    {
                        passengers += size;
                    }
                }

                if (departure.Status != DepartureStatus.Cancelled)
                {
                    departure.Status = passengers >= departure.Capacity && daysAway >= 0 ? DepartureStatus.SoldOut
                        : daysAway < 0 || passengers >= 6 ? DepartureStatus.Confirmed
                        : DepartureStatus.Scheduled;
                }
            }

            var reference = 100000;
            foreach (var booking in Bookings.OrderBy(booking => booking.BookedAt))
            {
                booking.Reference = $"VB-{++reference}";
            }
        }

        private void AddBooking(Departure departure, int size, DateTime bookedAt, BookingStatus status, List<ApplicationUser> agents, ILookup<Destination, Excursion> excursionsByDestination)
        {
            var traveller = faker.PickRandom(Travellers);
            var localeFaker = LocaleFaker(TravellerLocales.First(locale => locale.CountryCode == traveller.Address.CountryCode).Locale);
            var price = departure.PriceOverride ?? departure.Trip.BasePrice;
            var booking = new Booking
            {
                Reference = string.Empty,
                Traveller = traveller,
                Departure = departure,
                BookedAt = bookedAt,
                Status = status,
                SpecialRequests = faker.Random.Bool(0.15f) ? faker.PickRandom(SpecialRequests) : null,
                AssignedAgent = faker.Random.Bool(0.8f) ? faker.PickRandom(agents) : null,
                Version = faker.Random.Guid(),
            };

            for (var i = 0; i < size; i++)
            {
                var roomType = size switch
                {
                    1 => faker.Random.Bool(0.7f) ? RoomType.Single : RoomType.Twin,
                    2 => faker.Random.Bool(0.6f) ? RoomType.Double : RoomType.Twin,
                    3 => RoomType.Triple,
                    _ => RoomType.Twin,
                };
                var passenger = new BookingPassenger
                {
                    Booking = booking,
                    FirstName = i == 0 ? traveller.FirstName : localeFaker.Name.FirstName(),
                    LastName = i == 0 || faker.Random.Bool(0.6f) ? traveller.LastName : localeFaker.Name.LastName(),
                    DateOfBirth = i == 0 && traveller.DateOfBirth is { } dateOfBirth ? dateOfBirth : today.AddDays(-faker.Random.Int(365 * 12, 365 * 75)),
                    PassportNumber = faker.Random.Bool(0.7f) ? faker.Random.Replace("??#######").ToUpperInvariant() : null,
                    RoomType = roomType,
                    Price = roomType == RoomType.Single ? Math.Round(price * 1.25m / 10) * 10 : price,
                };
                booking.Passengers.Add(passenger);
                Passengers.Add(passenger);
            }

            if (status != BookingStatus.Cancelled && faker.Random.Bool(0.25f))
            {
                var options = excursionsByDestination[departure.Trip.Destination].ToList();
                foreach (var excursion in faker.PickRandom(options, Math.Min(options.Count, faker.Random.Int(1, 2))))
                {
                    var addOn = new BookingAddOn { Booking = booking, Excursion = excursion, Quantity = size, UnitPrice = excursion.Price };
                    booking.AddOns.Add(addOn);
                    AddOns.Add(addOn);
                }
            }

            booking.TotalAmount = booking.Passengers.Sum(passenger => passenger.Price) + booking.AddOns.Sum(addOn => addOn.UnitPrice * addOn.Quantity);
            Bookings.Add(booking);
            AddPayments(booking);
        }

        private void AddPayments(Booking booking)
        {
            var deposit = Math.Round(booking.TotalAmount * 0.2m, 2);

            if (booking.Status == BookingStatus.Pending && faker.Random.Bool(0.5f))
            {
                return;
            }

            AddPayment(booking, deposit, booking.BookedAt.AddMinutes(faker.Random.Int(1, 30)), PaymentStatus.Succeeded);

            switch (booking.Status)
            {
                case BookingStatus.Cancelled:
                    AddPayment(booking, deposit, booking.BookedAt.AddDays(faker.Random.Int(2, 40)), PaymentStatus.Refunded);
                    break;
                case BookingStatus.Confirmed or BookingStatus.Completed:
                    var balanceDue = booking.Departure.StartDate.ToDateTime(new TimeOnly(10, 0)).AddDays(-60);
                    var paidAt = balanceDue < booking.BookedAt ? booking.BookedAt.AddDays(1) : balanceDue.AddDays(-faker.Random.Int(0, 10));
                    if (paidAt <= Now)
                    {
                        AddPayment(booking, booking.TotalAmount - deposit, paidAt, PaymentStatus.Succeeded);
                    }

                    break;
            }
        }

        private void AddPayment(Booking booking, decimal amount, DateTime paidAt, PaymentStatus status)
        {
            var method = faker.Random.WeightedRandom([PaymentMethod.Card, PaymentMethod.BankTransfer, PaymentMethod.Voucher], [0.75f, 0.2f, 0.05f]);

            if (status == PaymentStatus.Succeeded && method == PaymentMethod.Card && faker.Random.Bool(0.03f))
            {
                AddPayment(booking, amount, paidAt.AddMinutes(-faker.Random.Int(5, 120)), PaymentStatus.Failed, method);
            }

            AddPayment(booking, amount, paidAt, status, method);
        }

        private void AddPayment(Booking booking, decimal amount, DateTime paidAt, PaymentStatus status, PaymentMethod method)
        {
            var payment = new Payment
            {
                Booking = booking,
                Amount = amount,
                Method = method,
                PaidAt = paidAt,
                ProviderReference = method switch
                {
                    PaymentMethod.Card => $"ch_{faker.Random.AlphaNumeric(24)}",
                    PaymentMethod.BankTransfer => $"TRF-{faker.Random.Replace("########")}",
                    _ => $"VCH-{faker.Random.AlphaNumeric(6).ToUpperInvariant()}",
                },
                Status = status,
            };
            booking.Payments.Add(payment);
            Payments.Add(payment);
        }

        private void GenerateReviews()
        {
            var reviewed = new HashSet<(Trip, Traveller)>();

            foreach (var booking in Bookings.Where(booking => booking.Status == BookingStatus.Completed))
            {
                var submittedAt = booking.Departure.EndDate.ToDateTime(new TimeOnly(0, 0)).AddDays(faker.Random.Int(3, 30)).AddMinutes(faker.Random.Int(420, 1380));
                if (submittedAt > Now || !faker.Random.Bool(0.35f) || !reviewed.Add((booking.Departure.Trip, booking.Traveller)))
                {
                    continue;
                }

                var rating = faker.Random.WeightedRandom([5, 4, 3, 2, 1], [0.55f, 0.3f, 0.1f, 0.03f, 0.02f]);
                var (title, body) = faker.PickRandom(ReviewTexts[rating]);
                Reviews.Add(new Review
                {
                    Trip = booking.Departure.Trip,
                    Traveller = booking.Traveller,
                    Rating = rating,
                    Title = title,
                    Body = faker.Random.Bool(0.9f) ? body : null,
                    SubmittedAt = submittedAt,
                    IsApproved = faker.Random.Bool((Now - submittedAt).TotalDays > 14 ? 0.92f : 0.3f),
                });
            }
        }

        private void GenerateRoomAllocations()
        {
            foreach (var departure in Departures)
            {
                var daysAway = departure.StartDate.DayNumber - today.DayNumber;

                foreach (var (accommodation, nights) in tripDetails[departure.Trip].Stays)
                {
                    RoomAllocations.Add(new RoomAllocation
                    {
                        Departure = departure,
                        Accommodation = accommodation,
                        Nights = nights,
                        RoomsReserved = (departure.Capacity + 1) / 2 + 1,
                        CostPerRoomNight = accommodation.StarRating switch
                        {
                            5 => faker.Random.Int(44, 84) * 5,
                            4 => faker.Random.Int(28, 44) * 5,
                            _ => faker.Random.Int(18, 28) * 5,
                        },
                        Status = departure.Status == DepartureStatus.Cancelled ? AllocationStatus.Released
                            : daysAway < 0 ? AllocationStatus.Confirmed
                            : daysAway <= 60 ? faker.Random.Bool(0.8f) ? AllocationStatus.Confirmed : AllocationStatus.Held
                            : daysAway <= 180 ? faker.Random.Bool(0.6f) ? AllocationStatus.Held : AllocationStatus.Requested
                            : AllocationStatus.Requested,
                    });
                }
            }
        }

        private void GenerateBookingUpdates()
        {
            foreach (var booking in Bookings.Where(_ => extras.Random.Bool(0.3f)))
            {
                var updatedAt = booking.BookedAt.AddDays(extras.Random.Int(1, 60)).AddMinutes(extras.Random.Int(0, 600));
                booking.UpdatedAt = updatedAt < Now ? updatedAt : null;
            }
        }

        private void GenerateInvoices()
        {
            var sequences = new Dictionary<Supplier, int>();

            foreach (var allocation in RoomAllocations.Where(allocation => allocation.Status == AllocationStatus.Confirmed && allocation.Departure.EndDate < today))
            {
                var issuedOn = allocation.Departure.EndDate.AddDays(faker.Random.Int(2, 10));
                if (issuedOn <= today)
                {
                    AddInvoice(allocation.Accommodation.Supplier, allocation.Departure, issuedOn, allocation.Nights * allocation.RoomsReserved * allocation.CostPerRoomNight, sequences);
                }
            }

            foreach (var supplier in Suppliers.Where(supplier => supplier.Type is SupplierType.Transport or SupplierType.Excursion && supplier.ContractStatus != ContractStatus.Prospect))
            {
                for (var month = 18; month >= 1; month--)
                {
                    var issuedOn = new DateOnly(today.Year, today.Month, 1).AddMonths(-month).AddDays(faker.Random.Int(0, 5));
                    AddInvoice(supplier, null, issuedOn, faker.Random.Int(300, 1800) * 5, sequences);
                }
            }

            Invoices.Sort((left, right) => left.IssuedOn.CompareTo(right.IssuedOn));
        }

        private void AddInvoice(Supplier supplier, Departure? departure, DateOnly issuedOn, decimal amount, Dictionary<Supplier, int> sequences)
        {
            sequences[supplier] = sequences.GetValueOrDefault(supplier) + 1;
            var dueOn = issuedOn.AddDays(30);

            Invoices.Add(new SupplierInvoice
            {
                Supplier = supplier,
                Departure = departure,
                InvoiceNumber = $"INV-{issuedOn:yy}-{sequences[supplier]:0000}",
                IssuedOn = issuedOn,
                DueOn = dueOn,
                Amount = amount,
                Status = dueOn < today
                    ? faker.Random.WeightedRandom([InvoiceStatus.Paid, InvoiceStatus.Disputed, InvoiceStatus.Approved], [0.9f, 0.04f, 0.06f])
                    : faker.Random.Bool() ? InvoiceStatus.Approved : InvoiceStatus.Received,
            });
        }

        private Country Country(string isoCode) => Countries.Single(country => country.IsoCode == isoCode);

        private string UniqueEmail(string email)
        {
            var candidate = email.ToLowerInvariant();
            var at = candidate.IndexOf('@');

            for (var suffix = 2; !emails.Add(candidate); suffix++)
            {
                candidate = $"{email[..at].ToLowerInvariant()}{suffix}{email[at..].ToLowerInvariant()}";
            }

            return candidate;
        }

        private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length].TrimEnd();
    }
}

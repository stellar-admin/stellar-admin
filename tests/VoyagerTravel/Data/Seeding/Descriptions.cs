using VoyagerTravel.Models;

namespace VoyagerTravel.Data.Seeding;

// The HTML text of destinations, trips, itinerary days and guide bios.
internal static class Descriptions
{
    public static readonly Dictionary<TripType, ItineraryPlan[]> ItineraryPlans = new()
    {
        [TripType.Adventure] =
        [
            new("Trek to the High Pass", "Today is the big climb: a steady ascent through ever-wilder country to the high pass, where the views open up in every direction.", ["A steady climb with regular rests to acclimatise", "Picnic lunch with a view from the top of the pass", "Descent to the valley in the late afternoon"]),
            new("Summit Day", "An early alarm, a head torch and a hot drink set you up for the trip's most demanding day, and its most rewarding view.", ["Pre-dawn start to reach the top before the cloud rolls in", "Celebration photos and a well-earned rest at the summit", "A long, careful descent with your guide setting the pace"]),
            new("Rest & Recovery", "After the big days, a chance to recharge your legs and catch up on sleep.", ["A lie-in and a late breakfast", "Optional stretching session or gentle walk", "Time to do laundry, read or simply sit with the view"]),
            new("Ridge Walk", "A day on the high ground, following a ridge with sweeping views on both sides.", ["Morning transfer to the trailhead", "Undulating walk along the ridge line", "Lunch at a viewpoint chosen by your guide"]),
            new("Into the Wilderness", "Leave the roads behind and head into remote country, where you're more likely to meet wildlife than other walkers.", ["Briefing on the route and what to carry", "A full day on trails through untouched landscape", "Look out for birds of prey and grazing animals"]),
            new("Glacier Day", "Strap on crampons and step onto the ice with a specialist guide to explore crevasses, meltwater channels and deep-blue ice.", ["Safety briefing and equipment fitting", "Guided walk on the ice with a qualified glacier guide", "Hot drinks back at base"]),
            new("River Crossing", "Follow a river upstream through a deep valley, fording side streams and stopping at swimming holes along the way.", ["Water shoes come in handy today", "A swim stop if the weather's warm", "Riverside lunch and plenty of time for photos"]),
            new("Valley Descent", "With the hard climbing behind you, today's route winds gently downhill through forest and farmland.", ["Mostly downhill walking on good paths", "Coffee and cake at a village café", "A free afternoon once you reach the hotel"]),
        ],
        [TripType.Cultural] =
        [
            new("Old Town Discovery", "Get your bearings on a walk through the old town with your guide, who knows its back streets, its stories and the best place for coffee.", ["Guided walk through the historic centre", "Hidden courtyards and viewpoints most visitors miss", "A free afternoon to return to your favourite spots"]),
            new("Temples & Traditions", "A day at the sacred places at the heart of local life, and the traditions that still shape it today.", ["An early visit before the crowds arrive", "Time with a local expert who explains the rituals and symbols", "Traditional lunch near the main temple"]),
            new("Meet the Artisans", "Spend the day with makers who keep traditional crafts alive, from workshops tucked down side streets to family studios outside town.", ["Demonstrations in two or three workshops", "Try the craft yourself under expert eyes", "Buy directly from the makers if something catches your eye"]),
            new("Museum Morning", "A guided morning at the finest museum in town, with a guide who brings the collection to life.", ["Skip-the-line entry ahead of the crowds", "A highlights tour of the collection's masterpieces", "An afternoon free to explore at your own pace"]),
            new("Village Homestay", "Spend the day with a family in a rural village, sharing a home-cooked meal and a glimpse of everyday life.", ["A welcome from the family and a walk around the village", "Help prepare lunch in the family kitchen", "Back at the hotel in time for a free evening"]),
            new("Palace Visit", "Step inside the grandest residence in the region, where a private guide leads you through the state rooms, the gardens and centuries of intrigue.", ["Guided tour of the state rooms", "A stroll through the formal gardens", "Afternoon tea or coffee nearby"]),
            new("Local Festival", "Join a local celebration of music, costumes and food stalls, timed to coincide with your visit wherever we can.", ["Background briefing from your guide", "Afternoon and evening at the festival", "Plenty of chances to try festival food"]),
            new("Market Day", "Dive into the colour and noise of the main market, where your guide introduces you to the stallholders and their produce.", ["Morning market walk with tastings", "Learn to haggle the friendly way", "Picnic lunch from your market finds"]),
        ],
        [TripType.Wellness] =
        [
            new("Morning Yoga & Spa", "Ease into the day with yoga led by a local teacher, then spend the afternoon being looked after at the spa.", ["Morning yoga for all levels", "A massage or spa treatment of your choice", "Healthy lunch by the pool"]),
            new("Beach Day", "Nothing to do and nowhere to be: a full day on one of the loveliest beaches in the area.", ["Loungers and umbrellas reserved for the group", "Swimming, snorkelling or a good book", "Seafood lunch at a beach café"]),
            new("Island Excursion", "Hop on a boat to a nearby island of quiet coves, clear water and an unhurried pace.", ["Morning boat transfer", "Swim stops and a walk across the island", "Back in time for sunset"]),
            new("Slow Morning", "No alarm today. Breakfast is served late, and the rest of the day is yours to fill, or not.", ["A late breakfast", "Optional guided meditation", "A free afternoon to rest or explore"]),
            new("Meditation Retreat", "A quieter day with a meditation teacher, in a peaceful setting away from the crowds.", ["Morning and afternoon meditation sessions", "A mindful walk through the gardens", "Light vegetarian lunch"]),
            new("Sunset Swim", "Spend the day at your own pace, then gather in the late afternoon for a swim as the sun goes down.", ["A free morning", "Gentle yoga in the late afternoon", "Sunset swim and drinks by the water"]),
            new("Free Day", "A day entirely to yourself. Your guide has plenty of ideas, from spa treatments to short walks and local cafés.", ["No scheduled activities", "Spa treatments can be booked locally", "Ask your guide for recommendations"]),
        ],
        [TripType.Cruise] =
        [
            new("Day at Sea", "A full day under way, with nothing on the horizon but water and the occasional pod of dolphins.", ["Talks on the region's wildlife and history", "A swim stop if the sea is calm", "Time on deck with a book and a view"]),
            new("Island Landing", "Go ashore on a remote island to walk its trails and see its wildlife at close range.", ["Small-boat landing on the island", "Guided walk with a naturalist", "Swimming or snorkelling from the beach"]),
            new("Snorkelling Stop", "Anchor in a sheltered bay and slip into the water to explore the reef and the life that crowds around it.", ["Snorkel briefing and kit fitting", "Two snorkelling sessions with a guide", "Lunch on deck between swims"]),
            new("Harbour Town", "Tie up in a pretty harbour town and spend the day exploring its lanes, markets and waterfront cafés.", ["Guided walk through the old port", "Free time to shop and explore", "Dinner at a harbourside restaurant"]),
            new("Sunrise Sail", "Up early to sail as the sun comes up, when the water is calm and the light is soft.", ["Coffee on deck at first light", "A morning sail along the coast", "An afternoon at anchor for swimming"]),
            new("Captain's Dinner", "A final day on the water, rounded off with a celebration dinner hosted by the captain and crew.", ["A last morning of swimming and sunbathing", "An afternoon at leisure", "The captain's dinner and farewell toasts"]),
        ],
        [TripType.CityBreak] =
        [
            new("Neighbourhood Walk", "Explore one of the city's most characterful neighbourhoods with your guide, well away from the tourist trail.", ["Street art, independent shops and local cafés", "Stories of the people who shaped the area", "Lunch at a neighbourhood favourite"]),
            new("Museum Highlights", "A guided tour of the city's essential museum, focused on the stories behind its greatest treasures.", ["Timed entry to beat the queues", "An expert-led highlights tour", "Free time in the museum shop and café"]),
            new("Food Tour", "Eat your way across the city with a local food guide, tasting the dishes that define it.", ["Six or seven tasting stops", "Bakeries, markets and tiny family restaurants", "Plenty of tips for the rest of your stay"]),
            new("Day Trip", "Leave the city behind for a day in the surrounding countryside, with your guide and private transport.", ["A morning drive out of the city", "A visit to a nearby town or landmark", "Back in the city by early evening"]),
            new("Free Day to Explore", "A day to see the city your way. Your guide shares a list of their favourite places before you head off.", ["No scheduled activities", "Use your transport pass to get around", "Recommendations for restaurants and bars"]),
            new("Rooftop Evening", "After a relaxed day, gather for drinks at a rooftop bar as the city lights come on.", ["A free day to explore", "Sunset drinks with views across the skyline", "Group dinner nearby"]),
        ],
        [TripType.Wildlife] =
        [
            new("Dawn Game Drive", "Set out at first light, when the animals are most active and the light is at its most beautiful.", ["Coffee and biscuits before you leave", "A morning drive with an expert tracker", "Back for a late breakfast"]),
            new("Walking Safari", "Explore on foot with a specialist ranger, learning to read tracks and to spot the small details you miss from a vehicle.", ["Safety briefing from your ranger", "A two- to three-hour guided walk", "Lunch and a rest through the heat of the day"]),
            new("Bird Watching", "A morning with a specialist birder, who knows exactly where to look for the region's most sought-after species.", ["An early start, with binoculars provided", "Two or three prime birding spots", "A checklist of everything you see"]),
            new("Boat Safari", "See wildlife from the water, gliding quietly past riverbanks and wetlands where animals come to drink.", ["Afternoon boat trip with a guide", "Birds, reptiles and animals at the water's edge", "Sundowner drinks on the water"]),
            new("Tracking Day", "Join expert trackers to follow the signs of one species through its habitat, for a rare chance to see it in the wild.", ["An early start with the tracking team", "Hours of patient searching, rewarded with time watching the animals", "Strict group sizes to keep disturbance to a minimum"]),
            new("Night Drive", "After dark, a completely different cast of animals comes out. Search for them by spotlight with a guide who knows their habits.", ["A free afternoon to rest", "An early dinner", "Spotlit drive after dark"]),
            new("Hide Afternoon", "Settle into a hide beside the water and let the animals come to you.", ["A free morning", "A quiet afternoon in the hide", "Back before dusk"]),
        ],
        [TripType.Culinary] =
        [
            new("Market & Cooking Class", "Shop for ingredients with a local chef, then head to their kitchen to cook, and eat, a feast of regional dishes.", ["Guided market walk with the chef", "A hands-on cooking class", "Lunch of everything you've made, with recipes to take home"]),
            new("Vineyard Visit", "Spend the day among the vines, meeting winemakers and tasting their wines where they're made.", ["Cellar tour with the winemaker", "A tasting of four or five wines", "Long lunch at the vineyard"]),
            new("Street Food Crawl", "As evening falls, follow your guide through the busiest food streets to try the snacks locals queue for.", ["A free day to explore", "Evening street food walk", "Ten or more tastings, so come hungry"]),
            new("Farm to Table", "Visit a family farm to see where your food comes from, then sit down to a lunch made with what was picked that morning.", ["Tour of the farm with the owners", "Help with the harvest, depending on the season", "Long lunch at the farmhouse table"]),
            new("Tasting Menu Evening", "Take the day at your own pace and save yourself for a tasting menu at one of the region's most celebrated restaurants.", ["A free day to explore", "A many-course tasting menu", "Optional wine pairing"]),
            new("Producer Visits", "Meet the producers behind the region's signature flavours, from cheesemakers and bakers to oil pressers and distillers.", ["Visits to three small producers", "Tastings at every stop", "Time to buy something to take home"]),
        ],
    };

    public static readonly string[] ItineraryTips =
    [
        "Bring a refillable water bottle; your guide knows where to fill it.",
        "Keep some small change handy for snacks and tips.",
        "Pack a light layer, as mornings and evenings can be cooler than you expect.",
        "Charge your camera tonight. You'll want it tomorrow.",
    ];

    public static readonly string[] GuideStories =
    [
        "Off the trail you'll find them behind a camera. Ask to see their photos from the last trip, and for tips on getting the shot yourself.",
        "They never travel without a good novel and a pocket notebook, and they're always happy to swap book recommendations over dinner.",
        "Ask them about the best street food in town. They always know, and on a free evening they'll happily take a few of you along.",
        "They trained as a naturalist and love spotting wildlife. Bring binoculars: they can name almost any bird by its call.",
        "They grew up in a family of mountain guides and learned to read the weather before they could read a map.",
    ];

    private static readonly string[] GuideQuotes =
    [
        "The best moments are rarely on the itinerary.",
        "Slow down, look up, and always say yes to the second cup of tea.",
        "Every place has a story. My job is to help you hear it.",
        "Good boots, good company and a good map. The rest takes care of itself.",
        "A trip is a success when the group is still laughing on the last day.",
        "I want people to leave with new friends, not just new photos.",
    ];

    private static readonly Dictionary<Climate, (string Note, string WhenToGo, string[] Pack)> Climates = new()
    {
        [Climate.Tropical] = (
            "Expect warm temperatures year-round, with a wetter season that keeps everything lush and green.",
            "It's warm all year. The dry season is the most comfortable time to travel, while the green season brings short afternoon downpours, fewer visitors and the lushest scenery.",
            ["Light, breathable clothes that dry quickly", "A compact rain jacket for afternoon showers", "Reef-safe sunscreen and insect repellent"]),
        [Climate.Arid] = (
            "Days are hot and dry and nights can be surprisingly cold, so pack layers.",
            "Spring and autumn are the most comfortable seasons, when the heat eases and the light is at its best. Summer days can be fierce, and desert nights are cold all year.",
            ["Loose layers that cover your arms and legs", "A warm fleece for the evenings", "A wide-brimmed hat, sunglasses and a refillable water bottle"]),
        [Climate.Temperate] = (
            "Spring and autumn bring mild weather and the best light for exploring.",
            "Spring and autumn bring mild weather, the best light and fewer crowds. Summers are warm and lively; winters are quieter and cooler, with shorter days.",
            ["Layers you can add or remove through the day", "A light waterproof jacket", "Comfortable shoes for long days on foot"]),
        [Climate.Mediterranean] = (
            "Summers are hot and dry; the shoulder seasons are ideal for walking and sightseeing.",
            "Spring and autumn are ideal for walking and sightseeing, with warm days and quieter streets. Summers are hot, dry and busy, and winters are mild but can be wet.",
            ["Sunscreen, a hat and sunglasses", "Swimwear for the sea or the hotel pool", "A light scarf to cover your shoulders in churches and religious sites"]),
        [Climate.Continental] = (
            "Summers are warm and winters are crisp, often with snow.",
            "Late spring and early autumn are the sweet spots, with comfortable temperatures and fewer crowds. Summers are warm and long; winters are crisp and often snowy.",
            ["A warm hat and gloves outside summer", "Sturdy, waterproof walking shoes", "A daypack for picnics and day hikes"]),
        [Climate.Polar] = (
            "Long summer days give way to dark winters lit by the northern lights.",
            "Summer brings near-endless daylight, the best hiking and the most wildlife. Winter is short on daylight but long on atmosphere, and the best time to see the northern lights.",
            ["Thermal base layers and a warm, windproof jacket", "Waterproof boots with a good grip", "An eye mask for the summer's midnight sun"]),
        [Climate.Alpine] = (
            "Mountain weather changes quickly, so come prepared for sun, wind and snow on the same day.",
            "Summer is the season for hiking, when the passes are open and the meadows are in flower. Winter belongs to skiers and snowshoers, and spring and autumn are quiet and changeable.",
            ["Layers, including a warm mid-layer and a waterproof shell", "Broken-in hiking boots", "Sunscreen and sunglasses: the sun is strong at altitude"]),
    };

    private static readonly Dictionary<TripType, (string Style, string Included)> TripStyles = new()
    {
        [TripType.Adventure] = ("This is an active trip. Expect early starts and full days outdoors, while a support vehicle carries your luggage between stops.", "A support vehicle for your luggage on active days"),
        [TripType.Cultural] = ("The pace is relaxed and the days are full: mornings at the main sights with your guide, afternoons free to linger in a museum or café.", "Entry to every site and museum on the itinerary"),
        [TripType.Wellness] = ("This is a trip for slowing down. Mornings start with yoga or a gentle walk, and there's plenty of unscheduled time to swim, read and rest.", "Daily yoga or meditation and one spa treatment"),
        [TripType.Cruise] = ("Much of the trip is spent on the water, with days of sailing, swimming stops and shore visits by small boat.", "All boat days, snorkelling gear and shore landings"),
        [TripType.CityBreak] = ("Guided walks help you get your bearings, and then you're free to explore on your own, with your guide's tips in hand.", "A public transport pass for getting around on your own"),
        [TripType.Wildlife] = ("Drives and walks are timed for early morning and late afternoon, when animals are most active, with a rest in the heat of the day. Small groups mean everyone gets a good view.", "Park and conservation fees and every wildlife excursion"),
        [TripType.Culinary] = ("Come hungry. Every day has something to taste, from market stalls and family kitchens to producers who have spent generations perfecting their craft.", "Cooking classes, tastings and producer visits"),
    };

    private static readonly (string Label, string Detail)[] Difficulties =
    [
        ("Easy", "Gentle walking on mostly level ground, suitable for anyone in reasonable health."),
        ("Moderate", "A few hours on foot most days, with some stairs and uneven paths."),
        ("Active", "Long days with hikes or rides of up to five hours. A good level of fitness helps."),
        ("Challenging", "Demanding days over rough terrain or at altitude. Regular exercise beforehand is strongly recommended."),
        ("Strenuous", "Long, demanding days with big climbs, often at altitude, for experienced and very fit travellers."),
    ];

    public static string ClimateNote(Climate climate) => Climates[climate].Note;

    public static string Destination(Destination destination, IReadOnlyList<Destination> children)
    {
        var climate = Climates[destination.Climate];
        var location = destination.ParentDestination is { } parent
            ? $"<p>{Encode(destination.Name)} is one of the highlights of {Encode(parent.Name)}, {Encode(destination.Country.Name)}.</p>"
            : children.Count > 0
                ? $"<p>Our trips in the region take in {List(children.Select(child => $"<strong>{Encode(child.Name)}</strong>"))}.</p>"
                : "";

        return $"<p>{Encode(destination.Summary!)}</p>{location}"
            + $"<h3>When to go</h3><p>{climate.WhenToGo}</p>"
            + $"<h3>What to pack</h3>{BulletList(climate.Pack)}";
    }

    public static string Trip(TripSeed seed, Trip trip, List<(Accommodation Accommodation, int Nights)> stays, IReadOnlyList<ItineraryDay> days)
    {
        var style = TripStyles[trip.TripType];
        var (label, detail) = Difficulties[trip.Difficulty - 1];
        var nights = stays.Sum(stay => stay.Nights);
        var included = new List<string>
        {
            $"{nights} nights' accommodation at {List(stays.Select(stay => $"<strong>{Encode(stay.Accommodation.Name)}</strong>"))}",
            Meals(days),
            "An expert local guide throughout",
            "All transport between destinations",
            style.Included,
        };

        return $"<p>{Encode(seed.Pitch)}</p><p>{style.Style}</p>"
            + $"<h3>What's included</h3><ul>{string.Concat(included.Select(item => $"<li>{item}</li>"))}</ul>"
            + $"<h3>Good to know</h3><p><strong>Difficulty: {label}.</strong> {detail}</p>"
            + $"<p><strong>Group size:</strong> up to {trip.MaxGroupSize} travellers.</p>"
            + $"<p><strong>Weather:</strong> {ClimateNote(trip.Destination.Climate)}</p>";
    }

    public static string ArrivalDay(Destination destination, Accommodation? accommodation) =>
        $"<p>Welcome to {Encode(destination.Name)}! Make your own way to {Hotel(accommodation)}, where your guide meets the group in the early evening.</p>"
        + BulletList(["Arrive at any time; rooms are usually ready from mid-afternoon", "A group briefing to run through the days ahead", "Welcome dinner at one of your guide's favourite restaurants"]);

    public static string DepartureDay() =>
        "<p>Time to say goodbye. The trip ends this morning when you check out of the hotel, with plenty of memories and a few new friends.</p>"
        + BulletList(["Check-out is in the late morning", "Airport transfers can be booked in advance", "Want to stay longer? We can arrange extra nights"]);

    public static string ItineraryDay(ItineraryPlan plan, string tip, Accommodation? movingTo) =>
        $"<p>{plan.Summary}</p>{BulletList(plan.Activities)}"
        + (movingTo is null ? "" : $"<p>Later in the day you travel on to {Hotel(movingTo)} for the next part of the trip.</p>")
        + $"<p><strong>Tip:</strong> {tip}</p>";

    public static string GuideBio(string firstName, int years, IEnumerable<Language> languages, string story, int quote) =>
        $"<p>{Encode(firstName)} has been leading small-group trips for {years} {(years == 1 ? "year" : "years")} and speaks {List(languages.Select(language => Encode(language.Name)))}.</p>"
        + $"<p>{story}</p>"
        + $"<blockquote><p>{GuideQuotes[quote % GuideQuotes.Length]}</p></blockquote>";

    // For example "7 breakfasts, 1 lunch and 3 dinners"
    private static string Meals(IReadOnlyList<ItineraryDay> days) =>
        List(new[] { (Models.Meals.Breakfast, "breakfast", "breakfasts"), (Models.Meals.Lunch, "lunch", "lunches"), (Models.Meals.Dinner, "dinner", "dinners") }
            .Select(meal => (Count: days.Count(day => day.Meals.HasFlag(meal.Item1)), meal.Item2, meal.Item3))
            .Where(meal => meal.Count > 0)
            .Select(meal => $"{meal.Count} {(meal.Count == 1 ? meal.Item2 : meal.Item3)}"));

    private static string Hotel(Accommodation? accommodation) =>
        accommodation is null ? "the hotel" : $"<strong>{Encode(accommodation.Name)}</strong>";

    private static string BulletList(IEnumerable<string> items) => $"<ul>{string.Concat(items.Select(item => $"<li>{item}</li>"))}</ul>";

    private static string List(IEnumerable<string> items)
    {
        var list = items.ToList();
        return list.Count == 1 ? list[0] : $"{string.Join(", ", list[..^1])} and {list[^1]}";
    }

    private static string Encode(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

internal sealed record ItineraryPlan(string Title, string Summary, string[] Activities);

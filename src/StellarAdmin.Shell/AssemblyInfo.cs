using Microsoft.AspNetCore.Mvc.ApplicationParts;

// Opts this assembly out of MVC's automatic application part discovery, so a host that merely
// references the package does not get the controllers and views. They are added explicitly by
// AddStellarAdmin().AddShell(). Requires GenerateRazorAssemblyInfo=false, since the Razor SDK
// would otherwise emit its own (conflicting) ProvideApplicationPartFactory attribute.
[assembly: ProvideApplicationPartFactory(typeof(NullApplicationPartFactory))]

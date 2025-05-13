using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Presentation.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext(options)
{
}


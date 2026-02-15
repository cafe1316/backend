using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine("⏭️  Database already seeded, skipping...");
            return;
        }

        Console.WriteLine("🌱 Starting seed...");

        // ==================== 1. Create Categories ====================
        Console.WriteLine("📁 Creating categories...");

        var coffeeBeans = new Category
        {
            Slug = "coffee-beans",
            Name = "Coffee Beans",
            Description = "Premium specialty coffee beans from around the world",
            DisplayOrder = 1,
            IsActive = true
        };

        var brewingGear = new Category
        {
            Slug = "brewing-gear",
            Name = "Brewing Gear",
            Description = "Professional coffee brewing equipment and tools",
            DisplayOrder = 2,
            IsActive = true
        };

        var accessories = new Category
        {
            Slug = "accessories",
            Name = "Accessories",
            Description = "Coffee accessories and essentials",
            DisplayOrder = 3,
            IsActive = true
        };

        await context.Categories.AddRangeAsync(coffeeBeans, brewingGear, accessories);
        await context.SaveChangesAsync();

        // ==================== 2. Create Subcategories ====================
        Console.WriteLine("📂 Creating subcategories...");

        // Coffee Beans subcategories
        var ethiopia = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "ethiopia", Name = "Ethiopia", DisplayOrder = 1, IsActive = true };
        var colombia = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "colombia", Name = "Colombia", DisplayOrder = 2, IsActive = true };
        var guatemala = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "guatemala", Name = "Guatemala", DisplayOrder = 3, IsActive = true };
        var brazil = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "brazil", Name = "Brazil", DisplayOrder = 4, IsActive = true };
        var kenya = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "kenya", Name = "Kenya", DisplayOrder = 5, IsActive = true };
        var costaRica = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "costa-rica", Name = "Costa Rica", DisplayOrder = 6, IsActive = true };
        var blend = new Subcategory { CategoryId = coffeeBeans.Id, Slug = "blend", Name = "Blend", DisplayOrder = 7, IsActive = true };

        // Brewing Gear subcategories
        var kettles = new Subcategory { CategoryId = brewingGear.Id, Slug = "kettles", Name = "Kettles", DisplayOrder = 1, IsActive = true };
        var drippers = new Subcategory { CategoryId = brewingGear.Id, Slug = "drippers", Name = "Drippers", DisplayOrder = 2, IsActive = true };
        var grinders = new Subcategory { CategoryId = brewingGear.Id, Slug = "grinders", Name = "Grinders", DisplayOrder = 3, IsActive = true };
        var espresso = new Subcategory { CategoryId = brewingGear.Id, Slug = "espresso-machines", Name = "Espresso Machines", DisplayOrder = 4, IsActive = true };
        var frenchPress = new Subcategory { CategoryId = brewingGear.Id, Slug = "french-press", Name = "French Press", DisplayOrder = 5, IsActive = true };

        // Accessories subcategories
        var cupsMugs = new Subcategory { CategoryId = accessories.Id, Slug = "cups-mugs", Name = "Cups & Mugs", DisplayOrder = 1, IsActive = true };
        var storage = new Subcategory { CategoryId = accessories.Id, Slug = "storage", Name = "Storage", DisplayOrder = 2, IsActive = true };
        var scales = new Subcategory { CategoryId = accessories.Id, Slug = "scales", Name = "Scales", DisplayOrder = 3, IsActive = true };
        var filters = new Subcategory { CategoryId = accessories.Id, Slug = "filters", Name = "Filters", DisplayOrder = 4, IsActive = true };

        await context.Subcategories.AddRangeAsync(
            ethiopia, colombia, guatemala, brazil, kenya, costaRica, blend,
            kettles, drippers, grinders, espresso, frenchPress,
            cupsMugs, storage, scales, filters
        );
        await context.SaveChangesAsync();

        // ==================== 3. Create Products ====================
        Console.WriteLine("☕ Creating products...");

        var products = new List<Product>();

        // ========== Coffee Beans (25 products) ==========

        // Ethiopia (5 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ETH-YIR-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = ethiopia.Id,
            Name = "Ethiopian Yirgacheffe",
            Slug = "ethiopian-yirgacheffe",
            Description = "Floral and citrus notes with a bright, clean finish. Grown at 1,800-2,200m elevation.",
            PriceCents = 2599,
            Currency = "AUD",
            Stock = 50,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Ethiopia,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 2000,
            Varietals = "Heirloom",
            HarvestYear = 2024,
            CuppingScore = 88,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ETH-SID-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = ethiopia.Id,
            Name = "Ethiopian Sidamo",
            Slug = "ethiopian-sidamo",
            Description = "Sweet and fruity with notes of blueberry and chocolate.",
            PriceCents = 2399,
            Currency = "AUD",
            Stock = 40,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Ethiopia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Natural,
            Altitude = 1850,
            Varietals = "Heirloom",
            HarvestYear = 2024,
            CuppingScore = 86,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ETH-HAR-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = ethiopia.Id,
            Name = "Ethiopian Harrar",
            Slug = "ethiopian-harrar",
            Description = "Wild and fruity with wine-like characteristics.",
            PriceCents = 4599,
            Currency = "AUD",
            Stock = 30,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.Ethiopia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Natural,
            Altitude = 1900,
            Varietals = "Heirloom",
            HarvestYear = 2024,
            CuppingScore = 87,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ETH-GUJ-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = ethiopia.Id,
            Name = "Ethiopian Guji",
            Slug = "ethiopian-guji",
            Description = "Complex fruit flavors with floral aromatics.",
            PriceCents = 2799,
            Currency = "AUD",
            Stock = 35,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Ethiopia,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 2100,
            Varietals = "Heirloom",
            HarvestYear = 2024,
            CuppingScore = 89,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ETH-LIM-1KG",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = ethiopia.Id,
            Name = "Ethiopian Limu",
            Slug = "ethiopian-limu",
            Description = "Balanced and sweet with notes of lemon and honey.",
            PriceCents = 8999,
            Currency = "AUD",
            Stock = 20,
            Unit = "bag",
            Weight = 1000,
            Origin = CoffeeOrigin.Ethiopia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1850,
            Varietals = "Heirloom",
            HarvestYear = 2024,
            CuppingScore = 85,
            IsActive = true
        });

        // Colombia (5 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "COL-SUP-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = colombia.Id,
            Name = "Colombia Supremo",
            Slug = "colombia-supremo",
            Description = "Rich and full-bodied with notes of caramel and nuts.",
            PriceCents = 2199,
            Currency = "AUD",
            Stock = 60,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Colombia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1700,
            Varietals = "Caturra, Castillo",
            HarvestYear = 2024,
            CuppingScore = 84,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "COL-HUI-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = colombia.Id,
            Name = "Colombia Huila",
            Slug = "colombia-huila",
            Description = "Bright acidity with notes of red apple and brown sugar.",
            PriceCents = 2499,
            Currency = "AUD",
            Stock = 45,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Colombia,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1850,
            Varietals = "Caturra",
            HarvestYear = 2024,
            CuppingScore = 87,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "COL-NAR-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = colombia.Id,
            Name = "Colombia Nariño",
            Slug = "colombia-narino",
            Description = "Complex and sweet with citrus and chocolate notes.",
            PriceCents = 4799,
            Currency = "AUD",
            Stock = 35,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.Colombia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 2000,
            Varietals = "Caturra, Typica",
            HarvestYear = 2024,
            CuppingScore = 88,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "COL-TOL-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = colombia.Id,
            Name = "Colombia Tolima",
            Slug = "colombia-tolima",
            Description = "Fruity and floral with a silky body.",
            PriceCents = 2699,
            Currency = "AUD",
            Stock = 40,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Colombia,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1900,
            Varietals = "Caturra",
            HarvestYear = 2024,
            CuppingScore = 86,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "COL-DEC-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = colombia.Id,
            Name = "Colombia Decaf",
            Slug = "colombia-decaf",
            Description = "Swiss water processed decaf with full flavor.",
            PriceCents = 2899,
            Currency = "AUD",
            Stock = 30,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Colombia,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1700,
            Varietals = "Caturra",
            HarvestYear = 2024,
            CuppingScore = 82,
            IsActive = true
        });

        // Guatemala (4 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GUA-ANT-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = guatemala.Id,
            Name = "Guatemala Antigua",
            Slug = "guatemala-antigua",
            Description = "Chocolatey and spicy with a smoky finish.",
            PriceCents = 2399,
            Currency = "AUD",
            Stock = 50,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Guatemala,
            RoastLevel = RoastLevel.Dark,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1600,
            Varietals = "Bourbon, Caturra",
            HarvestYear = 2024,
            CuppingScore = 85,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GUA-HUE-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = guatemala.Id,
            Name = "Guatemala Huehuetenango",
            Slug = "guatemala-huehuetenango",
            Description = "Bright and fruity with wine-like acidity.",
            PriceCents = 2599,
            Currency = "AUD",
            Stock = 40,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Guatemala,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1800,
            Varietals = "Bourbon, Typica",
            HarvestYear = 2024,
            CuppingScore = 87,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GUA-ATI-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = guatemala.Id,
            Name = "Guatemala Atitlán",
            Slug = "guatemala-atitlan",
            Description = "Full-bodied with chocolate and floral notes.",
            PriceCents = 4999,
            Currency = "AUD",
            Stock = 30,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.Guatemala,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1700,
            Varietals = "Bourbon",
            HarvestYear = 2024,
            CuppingScore = 86,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GUA-COB-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = guatemala.Id,
            Name = "Guatemala Cobán",
            Slug = "guatemala-coban",
            Description = "Delicate and sweet with fruity undertones.",
            PriceCents = 2499,
            Currency = "AUD",
            Stock = 35,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Guatemala,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1650,
            Varietals = "Bourbon, Caturra",
            HarvestYear = 2024,
            CuppingScore = 84,
            IsActive = true
        });

        // Brazil (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "BRA-SAN-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = brazil.Id,
            Name = "Brazil Santos",
            Slug = "brazil-santos",
            Description = "Smooth and nutty with low acidity.",
            PriceCents = 1999,
            Currency = "AUD",
            Stock = 70,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Brazil,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Natural,
            Altitude = 1100,
            Varietals = "Bourbon, Mundo Novo",
            HarvestYear = 2024,
            CuppingScore = 82,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "BRA-CER-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = brazil.Id,
            Name = "Brazil Cerrado",
            Slug = "brazil-cerrado",
            Description = "Sweet and chocolatey with a creamy body.",
            PriceCents = 3999,
            Currency = "AUD",
            Stock = 50,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.Brazil,
            RoastLevel = RoastLevel.Dark,
            ProcessingMethod = ProcessingMethod.Natural,
            Altitude = 1200,
            Varietals = "Catuaí",
            HarvestYear = 2024,
            CuppingScore = 83,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "BRA-SUL-1KG",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = brazil.Id,
            Name = "Brazil Sul de Minas",
            Slug = "brazil-sul-de-minas",
            Description = "Balanced with notes of caramel and hazelnut.",
            PriceCents = 7499,
            Currency = "AUD",
            Stock = 40,
            Unit = "bag",
            Weight = 1000,
            Origin = CoffeeOrigin.Brazil,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Natural,
            Altitude = 1150,
            Varietals = "Mundo Novo",
            HarvestYear = 2024,
            CuppingScore = 81,
            IsActive = true
        });

        // Kenya (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KEN-AA-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = kenya.Id,
            Name = "Kenya AA",
            Slug = "kenya-aa",
            Description = "Bright and complex with blackcurrant and citrus notes.",
            PriceCents = 2899,
            Currency = "AUD",
            Stock = 45,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Kenya,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1800,
            Varietals = "SL28, SL34",
            HarvestYear = 2024,
            CuppingScore = 89,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KEN-NYE-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = kenya.Id,
            Name = "Kenya Nyeri",
            Slug = "kenya-nyeri",
            Description = "Juicy and vibrant with berry and wine notes.",
            PriceCents = 3099,
            Currency = "AUD",
            Stock = 35,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Kenya,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1900,
            Varietals = "SL28",
            HarvestYear = 2024,
            CuppingScore = 90,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KEN-KIR-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = kenya.Id,
            Name = "Kenya Kirinyaga",
            Slug = "kenya-kirinyaga",
            Description = "Full-bodied with tomato and grapefruit notes.",
            PriceCents = 5999,
            Currency = "AUD",
            Stock = 30,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.Kenya,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1850,
            Varietals = "SL34, Ruiru 11",
            HarvestYear = 2024,
            CuppingScore = 88,
            IsActive = true
        });

        // Costa Rica (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CRI-TAR-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = costaRica.Id,
            Name = "Costa Rica Tarrazú",
            Slug = "costa-rica-tarrazu",
            Description = "Clean and bright with citrus and honey notes.",
            PriceCents = 2699,
            Currency = "AUD",
            Stock = 40,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.CostaRica,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1700,
            Varietals = "Caturra, Catuaí",
            HarvestYear = 2024,
            CuppingScore = 86,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CRI-WES-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = costaRica.Id,
            Name = "Costa Rica West Valley",
            Slug = "costa-rica-west-valley",
            Description = "Sweet and balanced with chocolate and fruit notes.",
            PriceCents = 2599,
            Currency = "AUD",
            Stock = 35,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.CostaRica,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Honey,
            Altitude = 1600,
            Varietals = "Caturra",
            HarvestYear = 2024,
            CuppingScore = 85,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CRI-CEN-500",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = costaRica.Id,
            Name = "Costa Rica Central Valley",
            Slug = "costa-rica-central-valley",
            Description = "Well-balanced with apple and caramel notes.",
            PriceCents = 4999,
            Currency = "AUD",
            Stock = 30,
            Unit = "bag",
            Weight = 500,
            Origin = CoffeeOrigin.CostaRica,
            RoastLevel = RoastLevel.Light,
            ProcessingMethod = ProcessingMethod.Washed,
            Altitude = 1500,
            Varietals = "Caturra, Catuaí",
            HarvestYear = 2024,
            CuppingScore = 84,
            IsActive = true
        });

        // Blend (2 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "BLD-HOU-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = blend.Id,
            Name = "House Blend",
            Slug = "house-blend",
            Description = "Balanced blend of South American and African beans.",
            PriceCents = 2199,
            Currency = "AUD",
            Stock = 80,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Other,
            RoastLevel = RoastLevel.Medium,
            ProcessingMethod = ProcessingMethod.Washed,
            HarvestYear = 2024,
            CuppingScore = 83,
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "BLD-ESP-250",
            CategoryId = coffeeBeans.Id,
            SubcategoryId = blend.Id,
            Name = "Espresso Blend",
            Slug = "espresso-blend",
            Description = "Rich and bold blend perfect for espresso.",
            PriceCents = 2399,
            Currency = "AUD",
            Stock = 70,
            Unit = "bag",
            Weight = 250,
            Origin = CoffeeOrigin.Other,
            RoastLevel = RoastLevel.Dark,
            ProcessingMethod = ProcessingMethod.Natural,
            HarvestYear = 2024,
            CuppingScore = 84,
            IsFeatured = true,
            IsActive = true
        });

        // ========== Brewing Gear (15 products) ==========

        // Kettles (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KET-FEL-STA",
            CategoryId = brewingGear.Id,
            SubcategoryId = kettles.Id,
            Name = "Fellow Stagg EKG Electric Kettle",
            Slug = "fellow-stagg-ekg-electric-kettle",
            Description = "Precision pour-over kettle with variable temperature control.",
            PriceCents = 19900,
            Currency = "AUD",
            Stock = 25,
            Unit = "piece",
            Brand = "Fellow",
            Capacity = 900,
            Material = "Stainless Steel",
            Color = "Matte Black",
            Specifications = "Temperature range: 135°F-212°F, 1200W",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KET-HAR-BUO",
            CategoryId = brewingGear.Id,
            SubcategoryId = kettles.Id,
            Name = "Hario V60 Buono Kettle",
            Slug = "hario-v60-buono-kettle",
            Description = "Classic gooseneck kettle for pour-over brewing.",
            PriceCents = 5900,
            Currency = "AUD",
            Stock = 40,
            Unit = "piece",
            Brand = "Hario",
            Capacity = 1200,
            Material = "Stainless Steel",
            Specifications = "Stovetop compatible",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "KET-BOD-GOO",
            CategoryId = brewingGear.Id,
            SubcategoryId = kettles.Id,
            Name = "Bodum Gooseneck Kettle",
            Slug = "bodum-gooseneck-kettle",
            Description = "Affordable gooseneck kettle for precise pouring.",
            PriceCents = 3900,
            Currency = "AUD",
            Stock = 35,
            Unit = "piece",
            Brand = "Bodum",
            Capacity = 1000,
            Material = "Stainless Steel",
            Specifications = "Stovetop compatible",
            IsActive = true
        });

        // Drippers (4 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "DRI-HAR-V60",
            CategoryId = brewingGear.Id,
            SubcategoryId = drippers.Id,
            Name = "Hario V60 Ceramic Dripper",
            Slug = "hario-v60-ceramic-dripper",
            Description = "Iconic cone-shaped dripper for clean, bright coffee.",
            PriceCents = 2900,
            Currency = "AUD",
            Stock = 60,
            Unit = "piece",
            Brand = "Hario",
            Size = "02",
            Material = "Ceramic",
            Color = "White",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "DRI-KAL-WAV",
            CategoryId = brewingGear.Id,
            SubcategoryId = drippers.Id,
            Name = "Kalita Wave 185",
            Slug = "kalita-wave-185",
            Description = "Flat-bottom dripper for consistent extraction.",
            PriceCents = 3500,
            Currency = "AUD",
            Stock = 45,
            Unit = "piece",
            Brand = "Kalita",
            Size = "185",
            Material = "Stainless Steel",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "DRI-CHE-CLA",
            CategoryId = brewingGear.Id,
            SubcategoryId = drippers.Id,
            Name = "Chemex Classic 6-Cup",
            Slug = "chemex-classic-6-cup",
            Description = "Elegant glass pour-over brewer with wooden collar.",
            PriceCents = 4900,
            Currency = "AUD",
            Stock = 35,
            Unit = "piece",
            Brand = "Chemex",
            Capacity = 900,
            Material = "Borosilicate Glass",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "DRI-ORI-DRI",
            CategoryId = brewingGear.Id,
            SubcategoryId = drippers.Id,
            Name = "Origami Dripper",
            Slug = "origami-dripper",
            Description = "Versatile dripper compatible with multiple filter types.",
            PriceCents = 3900,
            Currency = "AUD",
            Stock = 30,
            Unit = "piece",
            Brand = "Origami",
            Size = "Medium",
            Material = "Porcelain",
            Color = "White",
            IsActive = true
        });

        // Grinders (4 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GRI-BAR-ENC",
            CategoryId = brewingGear.Id,
            SubcategoryId = grinders.Id,
            Name = "Baratza Encore Grinder",
            Slug = "baratza-encore-grinder",
            Description = "Entry-level burr grinder with 40 grind settings.",
            PriceCents = 16900,
            Currency = "AUD",
            Stock = 20,
            Unit = "piece",
            Brand = "Baratza",
            Specifications = "40 grind settings, 8oz bean hopper",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GRI-1ZP-JX",
            CategoryId = brewingGear.Id,
            SubcategoryId = grinders.Id,
            Name = "1Zpresso JX Manual Grinder",
            Slug = "1zpresso-jx-manual-grinder",
            Description = "Premium manual grinder with 48mm conical burrs.",
            PriceCents = 13900,
            Currency = "AUD",
            Stock = 25,
            Unit = "piece",
            Brand = "1Zpresso",
            Specifications = "48mm conical burrs, 35g capacity",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GRI-HAR-SKE",
            CategoryId = brewingGear.Id,
            SubcategoryId = grinders.Id,
            Name = "Hario Skerton Pro",
            Slug = "hario-skerton-pro",
            Description = "Affordable manual grinder with ceramic burrs.",
            PriceCents = 5900,
            Currency = "AUD",
            Stock = 40,
            Unit = "piece",
            Brand = "Hario",
            Specifications = "Ceramic conical burrs, 100g capacity",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "GRI-FEL-ODE",
            CategoryId = brewingGear.Id,
            SubcategoryId = grinders.Id,
            Name = "Fellow Ode Brew Grinder",
            Slug = "fellow-ode-brew-grinder",
            Description = "Flat burr grinder designed for filter coffee.",
            PriceCents = 32900,
            Currency = "AUD",
            Stock = 15,
            Unit = "piece",
            Brand = "Fellow",
            Specifications = "64mm flat burrs, 31 grind settings",
            IsFeatured = true,
            IsActive = true
        });

        // French Press (2 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "FRE-BOD-CHA",
            CategoryId = brewingGear.Id,
            SubcategoryId = frenchPress.Id,
            Name = "Bodum Chambord French Press",
            Slug = "bodum-chambord-french-press",
            Description = "Classic French press with chrome-plated frame.",
            PriceCents = 4900,
            Currency = "AUD",
            Stock = 35,
            Unit = "piece",
            Brand = "Bodum",
            Capacity = 1000,
            Material = "Borosilicate Glass",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "FRE-ESP-P7",
            CategoryId = brewingGear.Id,
            SubcategoryId = frenchPress.Id,
            Name = "Espro P7 French Press",
            Slug = "espro-p7-french-press",
            Description = "Double micro-filter French press for clean coffee.",
            PriceCents = 9900,
            Currency = "AUD",
            Stock = 25,
            Unit = "piece",
            Brand = "Espro",
            Capacity = 950,
            Material = "Stainless Steel",
            IsFeatured = true,
            IsActive = true
        });

        // Espresso Machines (2 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ESP-BRE-BAM",
            CategoryId = brewingGear.Id,
            SubcategoryId = espresso.Id,
            Name = "Breville Bambino Plus",
            Slug = "breville-bambino-plus",
            Description = "Compact espresso machine with automatic milk frother.",
            PriceCents = 49900,
            Currency = "AUD",
            Stock = 10,
            Unit = "piece",
            Brand = "Breville",
            Specifications = "15 bar pump, 3 second heat-up",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "ESP-GAG-CLA",
            CategoryId = brewingGear.Id,
            SubcategoryId = espresso.Id,
            Name = "Gaggia Classic Pro",
            Slug = "gaggia-classic-pro",
            Description = "Semi-automatic espresso machine with commercial portafilter.",
            PriceCents = 54900,
            Currency = "AUD",
            Stock = 8,
            Unit = "piece",
            Brand = "Gaggia",
            Specifications = "15 bar pump, 58mm portafilter",
            IsFeatured = true,
            IsActive = true
        });

        // ========== Accessories (10 products) ==========

        // Cups & Mugs (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CUP-KEE-12",
            CategoryId = accessories.Id,
            SubcategoryId = cupsMugs.Id,
            Name = "KeepCup Brew 12oz",
            Slug = "keepcup-brew-12oz",
            Description = "Reusable glass coffee cup with cork band.",
            PriceCents = 2900,
            Currency = "AUD",
            Stock = 50,
            Unit = "piece",
            Brand = "KeepCup",
            Capacity = 340,
            Material = "Tempered Glass",
            Color = "Cork",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CUP-FEL-CAR",
            CategoryId = accessories.Id,
            SubcategoryId = cupsMugs.Id,
            Name = "Fellow Carter Move Mug",
            Slug = "fellow-carter-move-mug",
            Description = "Insulated travel mug with ceramic coating.",
            PriceCents = 3900,
            Currency = "AUD",
            Stock = 40,
            Unit = "piece",
            Brand = "Fellow",
            Capacity = 473,
            Material = "Stainless Steel",
            Color = "Matte Black",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "CUP-NOT-CER",
            CategoryId = accessories.Id,
            SubcategoryId = cupsMugs.Id,
            Name = "NotNeutral Lino Mug",
            Slug = "notneutral-lino-mug",
            Description = "Porcelain mug designed for latte art.",
            PriceCents = 1900,
            Currency = "AUD",
            Stock = 60,
            Unit = "piece",
            Brand = "NotNeutral",
            Capacity = 296,
            Material = "Porcelain",
            Color = "White",
            IsActive = true
        });

        // Storage (3 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "STO-AIR-VAC",
            CategoryId = accessories.Id,
            SubcategoryId = storage.Id,
            Name = "Airscape Coffee Canister",
            Slug = "airscape-coffee-canister",
            Description = "Vacuum-sealed canister to keep coffee fresh.",
            PriceCents = 3900,
            Currency = "AUD",
            Stock = 35,
            Unit = "piece",
            Brand = "Airscape",
            Capacity = 500,
            Material = "Stainless Steel",
            Specifications = "Holds 1lb of coffee beans",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "STO-FEL-ATC",
            CategoryId = accessories.Id,
            SubcategoryId = storage.Id,
            Name = "Fellow Atmos Vacuum Canister",
            Slug = "fellow-atmos-vacuum-canister",
            Description = "Twist-to-lock vacuum canister with date tracker.",
            PriceCents = 3500,
            Currency = "AUD",
            Stock = 30,
            Unit = "piece",
            Brand = "Fellow",
            Capacity = 400,
            Material = "Borosilicate Glass",
            Specifications = "Integrated date tracker",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "STO-COF-VAU",
            CategoryId = accessories.Id,
            SubcategoryId = storage.Id,
            Name = "Coffee Gator Stainless Canister",
            Slug = "coffee-gator-stainless-canister",
            Description = "Airtight canister with CO2 release valve.",
            PriceCents = 2900,
            Currency = "AUD",
            Stock = 40,
            Unit = "piece",
            Brand = "Coffee Gator",
            Capacity = 500,
            Material = "Stainless Steel",
            Specifications = "Built-in date tracker",
            IsActive = true
        });

        // Scales (2 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "SCA-HAR-DRI",
            CategoryId = accessories.Id,
            SubcategoryId = scales.Id,
            Name = "Hario V60 Drip Scale",
            Slug = "hario-v60-drip-scale",
            Description = "Digital scale with built-in timer for pour-over.",
            PriceCents = 5900,
            Currency = "AUD",
            Stock = 30,
            Unit = "piece",
            Brand = "Hario",
            Specifications = "2kg capacity, 0.1g precision, timer",
            IsFeatured = true,
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "SCA-ACE-PEA",
            CategoryId = accessories.Id,
            SubcategoryId = scales.Id,
            Name = "Acaia Pearl Coffee Scale",
            Slug = "acaia-pearl-coffee-scale",
            Description = "Professional coffee scale with Bluetooth connectivity.",
            PriceCents = 24900,
            Currency = "AUD",
            Stock = 15,
            Unit = "piece",
            Brand = "Acaia",
            Specifications = "2kg capacity, 0.1g precision, Bluetooth",
            IsFeatured = true,
            IsActive = true
        });

        // Filters (2 products)
        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "FIL-HAR-V60",
            CategoryId = accessories.Id,
            SubcategoryId = filters.Id,
            Name = "Hario V60 Paper Filters (100 pack)",
            Slug = "hario-v60-paper-filters-100-pack",
            Description = "Oxygen-bleached paper filters for V60 dripper.",
            PriceCents = 1200,
            Currency = "AUD",
            Stock = 100,
            Unit = "box",
            Brand = "Hario",
            Size = "02",
            Specifications = "100 filters per pack",
            IsActive = true
        });

        products.Add(new Product
        {
            Uuid = Guid.NewGuid(),
            Sku = "FIL-CHE-SQU",
            CategoryId = accessories.Id,
            SubcategoryId = filters.Id,
            Name = "Chemex Square Filters (100 pack)",
            Slug = "chemex-square-filters-100-pack",
            Description = "Bonded paper filters for Chemex brewers.",
            PriceCents = 1500,
            Currency = "AUD",
            Stock = 80,
            Unit = "box",
            Brand = "Chemex",
            Specifications = "100 filters per pack",
            IsActive = true
        });

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Created {products.Count} products");

        // ==================== 4. Add Flavor Notes ====================
        Console.WriteLine("🏷️  Adding flavor notes...");

        var flavorNotes = new List<ProductFlavorNote>
        {
            // Ethiopian Yirgacheffe
            new() { ProductId = products[0].Id, FlavorNote = FlavorNote.Floral },
            new() { ProductId = products[0].Id, FlavorNote = FlavorNote.Citrus },
            new() { ProductId = products[0].Id, FlavorNote = FlavorNote.Fruity },

            // Ethiopian Sidamo
            new() { ProductId = products[1].Id, FlavorNote = FlavorNote.Fruity },
            new() { ProductId = products[1].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[1].Id, FlavorNote = FlavorNote.Berry },

            // Ethiopian Harrar
            new() { ProductId = products[2].Id, FlavorNote = FlavorNote.Fruity },
            new() { ProductId = products[2].Id, FlavorNote = FlavorNote.Berry },

            // Ethiopian Guji
            new() { ProductId = products[3].Id, FlavorNote = FlavorNote.Floral },
            new() { ProductId = products[3].Id, FlavorNote = FlavorNote.Fruity },
            new() { ProductId = products[3].Id, FlavorNote = FlavorNote.Citrus },

            // Ethiopian Limu
            new() { ProductId = products[4].Id, FlavorNote = FlavorNote.Citrus },
            new() { ProductId = products[4].Id, FlavorNote = FlavorNote.Sweet },

            // Colombia Supremo
            new() { ProductId = products[5].Id, FlavorNote = FlavorNote.Caramel },
            new() { ProductId = products[5].Id, FlavorNote = FlavorNote.Nutty },

            // Colombia Huila
            new() { ProductId = products[6].Id, FlavorNote = FlavorNote.Fruity },
            new() { ProductId = products[6].Id, FlavorNote = FlavorNote.Sweet },
            new() { ProductId = products[6].Id, FlavorNote = FlavorNote.Caramel },

            // Colombia Nariño
            new() { ProductId = products[7].Id, FlavorNote = FlavorNote.Citrus },
            new() { ProductId = products[7].Id, FlavorNote = FlavorNote.Chocolate },

            // Colombia Tolima
            new() { ProductId = products[8].Id, FlavorNote = FlavorNote.Floral },
            new() { ProductId = products[8].Id, FlavorNote = FlavorNote.Fruity },

            // Guatemala Antigua
            new() { ProductId = products[10].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[10].Id, FlavorNote = FlavorNote.Spicy },

            // Guatemala Huehuetenango
            new() { ProductId = products[11].Id, FlavorNote = FlavorNote.Fruity },
            new() { ProductId = products[11].Id, FlavorNote = FlavorNote.Citrus },

            // Guatemala Atitlán
            new() { ProductId = products[12].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[12].Id, FlavorNote = FlavorNote.Floral },

            // Brazil Santos
            new() { ProductId = products[14].Id, FlavorNote = FlavorNote.Nutty },
            new() { ProductId = products[14].Id, FlavorNote = FlavorNote.Sweet },

            // Brazil Cerrado
            new() { ProductId = products[15].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[15].Id, FlavorNote = FlavorNote.Sweet },

            // Kenya AA
            new() { ProductId = products[17].Id, FlavorNote = FlavorNote.Berry },
            new() { ProductId = products[17].Id, FlavorNote = FlavorNote.Citrus },

            // Kenya Nyeri
            new() { ProductId = products[18].Id, FlavorNote = FlavorNote.Berry },
            new() { ProductId = products[18].Id, FlavorNote = FlavorNote.Fruity },

            // Costa Rica Tarrazú
            new() { ProductId = products[20].Id, FlavorNote = FlavorNote.Citrus },
            new() { ProductId = products[20].Id, FlavorNote = FlavorNote.Sweet },

            // House Blend
            new() { ProductId = products[23].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[23].Id, FlavorNote = FlavorNote.Nutty },
            new() { ProductId = products[23].Id, FlavorNote = FlavorNote.Caramel },

            // Espresso Blend
            new() { ProductId = products[24].Id, FlavorNote = FlavorNote.Chocolate },
            new() { ProductId = products[24].Id, FlavorNote = FlavorNote.Caramel }
        };

        await context.ProductFlavorNotes.AddRangeAsync(flavorNotes);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Added {flavorNotes.Count} flavor notes");

        // ==================== 5. Add Product Tags ====================
        Console.WriteLine("🏷️  Adding product tags...");

        // NOTE: 'limited_offer' has been removed from ProductTag enum
        // Replacing with 'seasonal' for Kenya Nyeri
        var productTags = new List<ProductTagMapping>
        {
            // Featured products
            new() { ProductId = products[0].Id, Tag = ProductTag.NewArrival },
            new() { ProductId = products[3].Id, Tag = ProductTag.Organic },
            new() { ProductId = products[6].Id, Tag = ProductTag.BestSeller },
            new() { ProductId = products[17].Id, Tag = ProductTag.Organic },
            new() { ProductId = products[18].Id, Tag = ProductTag.Seasonal }, // Was 'limited_offer'
            new() { ProductId = products[23].Id, Tag = ProductTag.BestSeller },
            new() { ProductId = products[24].Id, Tag = ProductTag.BestSeller },

            // Brewing gear
            new() { ProductId = products[25].Id, Tag = ProductTag.NewArrival },
            new() { ProductId = products[28].Id, Tag = ProductTag.BestSeller },
            new() { ProductId = products[31].Id, Tag = ProductTag.BestSeller },
            new() { ProductId = products[32].Id, Tag = ProductTag.NewArrival },

            // Accessories
            new() { ProductId = products[42].Id, Tag = ProductTag.BestSeller },
            new() { ProductId = products[44].Id, Tag = ProductTag.NewArrival }
        };

        await context.ProductTagMappings.AddRangeAsync(productTags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Added {productTags.Count} product tags");

        // ==================== 6. Add Product Images (New!) ====================
        Console.WriteLine("🖼️  Adding product images...");
        
        // Strategy: Iterate over tracked products and add images
        // Since we already saved Products via SaveChangesAsync() previously? 
        // Wait, looking at lines 1206-1207:
        // await context.Products.AddRangeAsync(products);
        // await context.SaveChangesAsync();
        // So products HAS IDs now.
        
        var productImages = new List<ProductImage>();
        
        foreach (var product in products)
        {
            string imageUrl = GetProductImageUrl(product.Name, product.Sku);
            productImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imageUrl,
                IsPrimary = true,
                DisplayOrder = 1
            });
        }
        
        await context.ProductImages.AddRangeAsync(productImages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Added {productImages.Count} product images");

        Console.WriteLine("🎉 Seed completed successfully!");
        Console.WriteLine($"   📁 {3} categories");
        Console.WriteLine($"   📂 {16} subcategories");
        Console.WriteLine($"   ☕ {products.Count} products");
        Console.WriteLine($"   🏷️  {flavorNotes.Count} flavor notes");
        Console.WriteLine($"   🏷️  {productTags.Count} product tags");
        Console.WriteLine($"   🖼️  {productImages.Count} product images");
    }

    private static string GetProductImageUrl(string productName, string sku)
    {
        // Using a switch expression for cleaner mapping based on exact product names
        // Fallback to Contains check if needed, but user provided specific mapping.
        
        return productName switch
        {
            // Ethiopia
            "Ethiopian Yirgacheffe" => "https://toffeecoffeeroasters.com/cdn/shop/products/1_bc245df0-db63-4b11-87f7-37dcd3a472e2_1080x.png?v=1675089677",
            "Ethiopian Sidamo" => "https://www.thebeancartel.com.au/cdn/shop/products/melbourne-specialty-coffee-roaster-single-origin-ethiopia.jpg?v=1641888734",
            "Ethiopian Harrar" => "https://blog.suvie.com/wp-content/uploads/2020/02/Harrar-beans.jpg",
            "Ethiopian Guji" => "https://cheekydevilcoffee.com.au/wp-content/uploads/2022/08/2-2-scaled-e1691637819560.jpg",
            "Ethiopian Limu" => "https://www.vellanero.com.au/cdn/shop/products/IMG_6315_grande.jpg?v=1454802434",
            
            // Colombia
            "Colombia Supremo" => "https://www.bakeandbrew.com.au/wp-content-6er4st/uploads/2013/09/Colombian-supremo.jpg",
            "Colombia Huila" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTqvPb1PK94fcxSDYtuHxvK7IR6q9334iknfQ&s",
            "Colombia Nariño" => "https://www.shutterstock.com/image-photo/colombia-narino-roasted-arabica-coffee-260nw-1187894506.jpg",
            "Colombia Tolima" => "https://specialtycoffeebrewing.com/wp-content/uploads/2024/04/colombian-coffee-beans.jpg",
            "Colombia Decaf" => "https://owleyecoffee.com/cdn/shop/files/ColombiaDecaf.jpg?v=1706635101",
            
            // Guatemala
            "Guatemala Antigua" => "https://coffeehero.com.au/cdn/shop/articles/2a0736c4a49458d2a920231ccef7eddb_2048x2048.jpg?v=1625059744",
            "Guatemala Huehuetenango" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTS1wxOB_r1mrqB9JGLQGeS31GSazhuDuIo1g&s",
            "Guatemala Atitlán" => "https://espressocoffeeguide.com/wp-content/uploads/2010/05/guatemalaorganic-coffee-beans.jpg",
            "Guatemala Cobán" => "https://m.media-amazon.com/images/I/81Hr7hiZrOL.jpg",
            
            // Brazil
            "Brazil Santos" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTuaDDAy_lCVb5rYRxYRIRUFjQgW94ecr1Hvg&s",
            "Brazil Cerrado" => "https://svtea.com/cdn/shop/products/coffee1-1_xl_60ed3267-9596-486c-ab58-a1517ae908a8.jpg?v=1654785468",
            "Brazil Sul de Minas" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPrVUWEzvg-77bPM52H5bwY85wyEKjBbFXfg&s",

            // Kenya
            "Kenya AA" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQNQBbgimMAlkcKrHQmDEILsF90eTBO5SlQHw&s",
            "Kenya Nyeri" => "https://islandcruiserscoffee.com/cdn/shop/files/IMG-2452.heic?v=1764767593&width=1946",
            "Kenya Kirinyaga" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSif4r7dgPLCISpY6NRBgfxJWKXL3vrT5x5Vg&s",

            // Costa Rica
            "Costa Rica Tarrazú" => "https://images.squarespace-cdn.com/content/v1/5e4b33ab386d0459c86b5174/1605063065468-W89PDLBIV1QZLCRM2DTE/Costa+Rica_DSC_8458_c.jpg",
            "Costa Rica West Valley" => "https://alarosteri.se/cdn/shop/files/40aabd_dec0a0837b60431ebd8916d957efe58f_mv2_green_wv.jpg?v=1728827482&width=1445",
            "Costa Rica Central Valley" => "https://burmancoffee.com/wp-content/uploads/2026/01/Costa-Rica-Vida.jpg",

            // Blends
            "House Blend" => "https://www.ciscoscoffee.com.au/wp-content/uploads/2021/10/Ciscos-Coffee-House-Blend-Coffee.jpg",
            "Espresso Blend" => "https://static1.squarespace.com/static/6111f6f4a45ca157a14b224a/6111f6f8a45ca157a14b2336/6111fdb8005df17c5ef34a4b/1764300175164/249235573_750872152417439_7244262349121771721_n.jpg?format=1500w",
            
            // Kettles
            "Fellow Stagg EKG Electric Kettle" => "https://www.ciscoscoffee.com.au/wp-content/uploads/2024/05/Ciscos-Coffee-Fellow-Stagg-EKG-Electric-Kettle-Smoke-Green.jpg",
            "Hario V60 Buono Kettle" => "https://harioaustralia.com.au/cdn/shop/files/VKBN_04.png?v=1763104844&width=2048",
            "Bodum Gooseneck Kettle" => "https://cb.scene7.com/is/image/Crate//MelittaPrcsPourKttlSSF23_VND?$web_search_sm$&$web_pdp_carousel_low$",

            // Drippers
            "Hario V60 Ceramic Dripper" => "https://harioaustralia.com.au/cdn/shop/files/VDC-02-SPB_00.png?v=1763199867&width=2048",
            "Kalita Wave 185" => "https://images.getrecipekit.com/20220413173314-brewing-20a-20kalita-20wave-20185-20square.JPG?aspect_ratio=4:3&quality=90&",
            "Chemex Classic 6-Cup" => "https://d2og65f1kwx1z6.cloudfront.net/wp-content/uploads/2021/05/quest-chemex-6-cup-product.jpg",
            "Origami Dripper" => "https://origamidripper.au/cdn/shop/files/Origami_HolderAS_WhiteSplash3_2048x.jpg?v=1707975208",

            // Grinders
            "Baratza Encore Grinder" => "https://fivesenses.com.au/cdn/shop/products/Encore-Both-WEBSITE.png?v=1655347055",
            "1Zpresso JX Manual Grinder" => "https://baristawarehouse.com.au/cdn/shop/files/1Zpresso-JX-Pro-S-Hand-Coffee-Grinder_600x600.jpg?v=1751860679",
            "Hario Skerton Pro" => "https://coffeemachinespecialist.com.au/wp-content/uploads/09._Hario_Coffee_Mill_____Skerton_PRO-1-510x510.png",
            "Fellow Ode Brew Grinder" => "https://images.squarespace-cdn.com/content/v1/6464abb2deb72f00037909ee/1701925003248-EOKUQWTTEZ5AGET4ZXOV/Fellow-Ode-Brew-Gen-2-Coffee-Grinder-White_600x600.jpg?format=1000w",

            // French Press
            "Bodum Chambord French Press" => "https://www.kitchenwarehouse.com.au/_next/image?url=https%3A%2F%2Fmedia.kitchenwarehouse.com.au%2Fkitchenwarehouse%2Fimage%2Fupload%2Fc_fill%2Cg_face%2Cw_auto%2Cf_auto%2Cq_auto%2Ft_PDP_2000x2000%2FSupplier%2520Images%2520%2F2000px%2FBodum-Chambord-French-Press-8-Cup_2_2000px.jpg%3Fimagetype%3Dpdp_full&w=828&q=75",
            "Espro P7 French Press" => "https://www.williams-sonoma.com.au/site/WS/Product%20Images/espro-stainless-steel-french-press-202437-0009-espro-p7-french-press-z.jpg?resizeid=93&resizeh=450&resizew=450",

            // Espresso Machines
            "Breville Bambino Plus" => "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRVc3V2SgNvir1lbqntl_yjst13pLIHocCyXw&s",
            "Gaggia Classic Pro" => "https://www.gaggia.com.au/wp-content/uploads/2017/01/Gaggia-ClassicEvo_-Automatic-coffee-machine.png",
            
            // Cups & Mugs
            "KeepCup Brew 12oz" => "https://au.keepcup.com/cdn/shop/files/KeepCup-Brew-Cork_Moonlight_M_12oz-inhand.jpg?v=1758013639&width=1000",
            "Fellow Carter Move Mug" => "https://www.essentialutensil.au/cdn/shop/files/stone_life_14_1.jpg?v=1734410920",
            "NotNeutral Lino Mug" => "https://m.media-amazon.com/images/I/31+ZrUIdS2L.jpg",

            // Storage
            "Airscape Coffee Canister" => "https://alternativebrewing.com.au/cdn/shop/files/Airscape-Classic-Matte-Blue-7_-Small_600x600_018135f0-8a22-4428-878e-860a7e0e7cb0_600x.webp?v=1751520760",
            "Fellow Atmos Vacuum Canister" => "https://cremacoffeegarage.com.au/media/catalog/product/cache/1f5a9c70549b661653ba94d9b4c0b627/f/e/fellow-atmos-vacuum-bean-canister-glass-12l450g.jpg",
            "Coffee Gator Stainless Canister" => "https://m.media-amazon.com/images/I/81ujt7wTDjL.jpg",

            // Scales
            "Hario V60 Drip Scale" => "https://m.media-amazon.com/images/I/51ljyAGEu1L.jpg",
            "Acaia Pearl Coffee Scale" => "https://alternativebrewing.com.au/cdn/shop/files/Acaia-Pearl-2021-Brewing-Scale_c9e2ac04-cb17-4e42-be5e-c9073b411cef_600x.jpg?v=1698814016",

            // Filters
            "Hario V60 Paper Filters (100 pack)" => "https://m.media-amazon.com/images/I/61pFczU9XtL.jpg",
            "Chemex Square Filters (100 pack)" => "https://www.dairybeanz.co.nz/cdn/shop/files/chemex-square-filters-white-100.jpg?v=1732928707&width=1445",

            // Default Fallback
            _ => "https://images.unsplash.com/photo-1497935586351-b67a49e012bf?ixlib=rb-4.0.3&auto=format&fit=crop&w=1471&q=80"
        };
    }
}

import db from '../src/db';
import {
    categories,
    subcategories,
    products,
    productImages,
    productFlavorNotes,
    productTags
} from './schema';

async function seed() {
    console.log('🌱 Starting seed...');

    // ==================== 清空现有数据 ====================
    console.log('🗑️  Clearing existing data...');
    await db.delete(productTags);
    await db.delete(productFlavorNotes);
    await db.delete(productImages);
    await db.delete(products);
    await db.delete(subcategories);
    await db.delete(categories);

    // ==================== 1. 创建分类 ====================
    console.log('📁 Creating categories...');

    const [coffeeBeans, brewingGear, accessories] = await db.insert(categories).values([
        {
            slug: 'coffee-beans',
            name: 'Coffee Beans',
            description: 'Premium specialty coffee beans from around the world',
            displayOrder: 1,
            isActive: true
        },
        {
            slug: 'brewing-gear',
            name: 'Brewing Gear',
            description: 'Professional coffee brewing equipment and tools',
            displayOrder: 2,
            isActive: true
        },
        {
            slug: 'accessories',
            name: 'Accessories',
            description: 'Coffee accessories and essentials',
            displayOrder: 3,
            isActive: true
        }
    ]).returning();

    // ==================== 2. 创建子分类 ====================
    console.log('📂 Creating subcategories...');

    // Coffee Beans 子分类
    const [ethiopia, colombia, guatemala, brazil, kenya, costaRica, blend] = await db.insert(subcategories).values([
        { categoryId: coffeeBeans.id, slug: 'ethiopia', name: 'Ethiopia', displayOrder: 1 },
        { categoryId: coffeeBeans.id, slug: 'colombia', name: 'Colombia', displayOrder: 2 },
        { categoryId: coffeeBeans.id, slug: 'guatemala', name: 'Guatemala', displayOrder: 3 },
        { categoryId: coffeeBeans.id, slug: 'brazil', name: 'Brazil', displayOrder: 4 },
        { categoryId: coffeeBeans.id, slug: 'kenya', name: 'Kenya', displayOrder: 5 },
        { categoryId: coffeeBeans.id, slug: 'costa-rica', name: 'Costa Rica', displayOrder: 6 },
        { categoryId: coffeeBeans.id, slug: 'blend', name: 'Blend', displayOrder: 7 }
    ]).returning();

    // Brewing Gear 子分类
    const [kettles, drippers, grinders, espresso, frenchPress] = await db.insert(subcategories).values([
        { categoryId: brewingGear.id, slug: 'kettles', name: 'Kettles', displayOrder: 1 },
        { categoryId: brewingGear.id, slug: 'drippers', name: 'Drippers', displayOrder: 2 },
        { categoryId: brewingGear.id, slug: 'grinders', name: 'Grinders', displayOrder: 3 },
        { categoryId: brewingGear.id, slug: 'espresso-machines', name: 'Espresso Machines', displayOrder: 4 },
        { categoryId: brewingGear.id, slug: 'french-press', name: 'French Press', displayOrder: 5 }
    ]).returning();

    // Accessories 子分类
    const [cupsMugs, storage, scales, filters] = await db.insert(subcategories).values([
        { categoryId: accessories.id, slug: 'cups-mugs', name: 'Cups & Mugs', displayOrder: 1 },
        { categoryId: accessories.id, slug: 'storage', name: 'Storage', displayOrder: 2 },
        { categoryId: accessories.id, slug: 'scales', name: 'Scales', displayOrder: 3 },
        { categoryId: accessories.id, slug: 'filters', name: 'Filters', displayOrder: 4 }
    ]).returning();

    // ==================== 3. 创建商品 ====================
    console.log('☕ Creating products...');

    // ========== Coffee Beans (25 products) ==========

    const coffeeProducts = await db.insert(products).values([
        // Ethiopia (5 products)
        {
            sku: 'ETH-YIR-250',
            categoryId: coffeeBeans.id,
            subcategoryId: ethiopia.id,
            name: 'Ethiopian Yirgacheffe',
            slug: 'ethiopian-yirgacheffe',
            description: 'Floral and citrus notes with a bright, clean finish. Grown at 1,800-2,200m elevation.',
            priceCents: 2599,
            stock: 50,
            unit: 'bag',
            weight: 250,
            origin: 'ethiopia',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 2000,
            varietals: 'Heirloom',
            harvestYear: 2024,
            cuppingScore: 88,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'ETH-SID-250',
            categoryId: coffeeBeans.id,
            subcategoryId: ethiopia.id,
            name: 'Ethiopian Sidamo',
            slug: 'ethiopian-sidamo',
            description: 'Sweet and fruity with notes of blueberry and chocolate.',
            priceCents: 2399,
            stock: 40,
            unit: 'bag',
            weight: 250,
            origin: 'ethiopia',
            roastLevel: 'medium',
            processingMethod: 'natural',
            altitude: 1850,
            varietals: 'Heirloom',
            harvestYear: 2024,
            cuppingScore: 86,
            isActive: true
        },
        {
            sku: 'ETH-HAR-500',
            categoryId: coffeeBeans.id,
            subcategoryId: ethiopia.id,
            name: 'Ethiopian Harrar',
            slug: 'ethiopian-harrar',
            description: 'Wild and fruity with wine-like characteristics.',
            priceCents: 4599,
            stock: 30,
            unit: 'bag',
            weight: 500,
            origin: 'ethiopia',
            roastLevel: 'medium',
            processingMethod: 'natural',
            altitude: 1900,
            varietals: 'Heirloom',
            harvestYear: 2024,
            cuppingScore: 87,
            isActive: true
        },
        {
            sku: 'ETH-GUJ-250',
            categoryId: coffeeBeans.id,
            subcategoryId: ethiopia.id,
            name: 'Ethiopian Guji',
            slug: 'ethiopian-guji',
            description: 'Complex fruit flavors with floral aromatics.',
            priceCents: 2799,
            stock: 35,
            unit: 'bag',
            weight: 250,
            origin: 'ethiopia',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 2100,
            varietals: 'Heirloom',
            harvestYear: 2024,
            cuppingScore: 89,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'ETH-LIM-1KG',
            categoryId: coffeeBeans.id,
            subcategoryId: ethiopia.id,
            name: 'Ethiopian Limu',
            slug: 'ethiopian-limu',
            description: 'Balanced and sweet with notes of lemon and honey.',
            priceCents: 8999,
            stock: 20,
            unit: 'bag',
            weight: 1000,
            origin: 'ethiopia',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1850,
            varietals: 'Heirloom',
            harvestYear: 2024,
            cuppingScore: 85,
            isActive: true
        },

        // Colombia (5 products)
        {
            sku: 'COL-SUP-250',
            categoryId: coffeeBeans.id,
            subcategoryId: colombia.id,
            name: 'Colombia Supremo',
            slug: 'colombia-supremo',
            description: 'Rich and full-bodied with notes of caramel and nuts.',
            priceCents: 2199,
            stock: 60,
            unit: 'bag',
            weight: 250,
            origin: 'colombia',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1700,
            varietals: 'Caturra, Castillo',
            harvestYear: 2024,
            cuppingScore: 84,
            isActive: true
        },
        {
            sku: 'COL-HUI-250',
            categoryId: coffeeBeans.id,
            subcategoryId: colombia.id,
            name: 'Colombia Huila',
            slug: 'colombia-huila',
            description: 'Bright acidity with notes of red apple and brown sugar.',
            priceCents: 2499,
            stock: 45,
            unit: 'bag',
            weight: 250,
            origin: 'colombia',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1850,
            varietals: 'Caturra',
            harvestYear: 2024,
            cuppingScore: 87,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'COL-NAR-500',
            categoryId: coffeeBeans.id,
            subcategoryId: colombia.id,
            name: 'Colombia Nariño',
            slug: 'colombia-narino',
            description: 'Complex and sweet with citrus and chocolate notes.',
            priceCents: 4799,
            stock: 35,
            unit: 'bag',
            weight: 500,
            origin: 'colombia',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 2000,
            varietals: 'Caturra, Typica',
            harvestYear: 2024,
            cuppingScore: 88,
            isActive: true
        },
        {
            sku: 'COL-TOL-250',
            categoryId: coffeeBeans.id,
            subcategoryId: colombia.id,
            name: 'Colombia Tolima',
            slug: 'colombia-tolima',
            description: 'Fruity and floral with a silky body.',
            priceCents: 2699,
            stock: 40,
            unit: 'bag',
            weight: 250,
            origin: 'colombia',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1900,
            varietals: 'Caturra',
            harvestYear: 2024,
            cuppingScore: 86,
            isActive: true
        },
        {
            sku: 'COL-DEC-250',
            categoryId: coffeeBeans.id,
            subcategoryId: colombia.id,
            name: 'Colombia Decaf',
            slug: 'colombia-decaf',
            description: 'Swiss water processed decaf with full flavor.',
            priceCents: 2899,
            stock: 30,
            unit: 'bag',
            weight: 250,
            origin: 'colombia',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1700,
            varietals: 'Caturra',
            harvestYear: 2024,
            cuppingScore: 82,
            isActive: true
        },

        // Guatemala (4 products)
        {
            sku: 'GUA-ANT-250',
            categoryId: coffeeBeans.id,
            subcategoryId: guatemala.id,
            name: 'Guatemala Antigua',
            slug: 'guatemala-antigua',
            description: 'Chocolatey and spicy with a smoky finish.',
            priceCents: 2399,
            stock: 50,
            unit: 'bag',
            weight: 250,
            origin: 'guatemala',
            roastLevel: 'dark',
            processingMethod: 'washed',
            altitude: 1600,
            varietals: 'Bourbon, Caturra',
            harvestYear: 2024,
            cuppingScore: 85,
            isActive: true
        },
        {
            sku: 'GUA-HUE-250',
            categoryId: coffeeBeans.id,
            subcategoryId: guatemala.id,
            name: 'Guatemala Huehuetenango',
            slug: 'guatemala-huehuetenango',
            description: 'Bright and fruity with wine-like acidity.',
            priceCents: 2599,
            stock: 40,
            unit: 'bag',
            weight: 250,
            origin: 'guatemala',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1800,
            varietals: 'Bourbon, Typica',
            harvestYear: 2024,
            cuppingScore: 87,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'GUA-ATI-500',
            categoryId: coffeeBeans.id,
            subcategoryId: guatemala.id,
            name: 'Guatemala Atitlán',
            slug: 'guatemala-atitlan',
            description: 'Full-bodied with chocolate and floral notes.',
            priceCents: 4999,
            stock: 30,
            unit: 'bag',
            weight: 500,
            origin: 'guatemala',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1700,
            varietals: 'Bourbon',
            harvestYear: 2024,
            cuppingScore: 86,
            isActive: true
        },
        {
            sku: 'GUA-COB-250',
            categoryId: coffeeBeans.id,
            subcategoryId: guatemala.id,
            name: 'Guatemala Cobán',
            slug: 'guatemala-coban',
            description: 'Delicate and sweet with fruity undertones.',
            priceCents: 2499,
            stock: 35,
            unit: 'bag',
            weight: 250,
            origin: 'guatemala',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1650,
            varietals: 'Bourbon, Caturra',
            harvestYear: 2024,
            cuppingScore: 84,
            isActive: true
        },

        // Brazil (3 products)
        {
            sku: 'BRA-SAN-250',
            categoryId: coffeeBeans.id,
            subcategoryId: brazil.id,
            name: 'Brazil Santos',
            slug: 'brazil-santos',
            description: 'Smooth and nutty with low acidity.',
            priceCents: 1999,
            stock: 70,
            unit: 'bag',
            weight: 250,
            origin: 'brazil',
            roastLevel: 'medium',
            processingMethod: 'natural',
            altitude: 1100,
            varietals: 'Bourbon, Mundo Novo',
            harvestYear: 2024,
            cuppingScore: 82,
            isActive: true
        },
        {
            sku: 'BRA-CER-500',
            categoryId: coffeeBeans.id,
            subcategoryId: brazil.id,
            name: 'Brazil Cerrado',
            slug: 'brazil-cerrado',
            description: 'Sweet and chocolatey with a creamy body.',
            priceCents: 3999,
            stock: 50,
            unit: 'bag',
            weight: 500,
            origin: 'brazil',
            roastLevel: 'dark',
            processingMethod: 'natural',
            altitude: 1200,
            varietals: 'Catuaí',
            harvestYear: 2024,
            cuppingScore: 83,
            isActive: true
        },
        {
            sku: 'BRA-SUL-1KG',
            categoryId: coffeeBeans.id,
            subcategoryId: brazil.id,
            name: 'Brazil Sul de Minas',
            slug: 'brazil-sul-de-minas',
            description: 'Balanced with notes of caramel and hazelnut.',
            priceCents: 7499,
            stock: 40,
            unit: 'bag',
            weight: 1000,
            origin: 'brazil',
            roastLevel: 'medium',
            processingMethod: 'natural',
            altitude: 1150,
            varietals: 'Mundo Novo',
            harvestYear: 2024,
            cuppingScore: 81,
            isActive: true
        },

        // Kenya (3 products)
        {
            sku: 'KEN-AA-250',
            categoryId: coffeeBeans.id,
            subcategoryId: kenya.id,
            name: 'Kenya AA',
            slug: 'kenya-aa',
            description: 'Bright and complex with blackcurrant and citrus notes.',
            priceCents: 2899,
            stock: 45,
            unit: 'bag',
            weight: 250,
            origin: 'kenya',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1800,
            varietals: 'SL28, SL34',
            harvestYear: 2024,
            cuppingScore: 89,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'KEN-NYE-250',
            categoryId: coffeeBeans.id,
            subcategoryId: kenya.id,
            name: 'Kenya Nyeri',
            slug: 'kenya-nyeri',
            description: 'Juicy and vibrant with berry and wine notes.',
            priceCents: 3099,
            stock: 35,
            unit: 'bag',
            weight: 250,
            origin: 'kenya',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1900,
            varietals: 'SL28',
            harvestYear: 2024,
            cuppingScore: 90,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'KEN-KIR-500',
            categoryId: coffeeBeans.id,
            subcategoryId: kenya.id,
            name: 'Kenya Kirinyaga',
            slug: 'kenya-kirinyaga',
            description: 'Full-bodied with tomato and grapefruit notes.',
            priceCents: 5999,
            stock: 30,
            unit: 'bag',
            weight: 500,
            origin: 'kenya',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1850,
            varietals: 'SL34, Ruiru 11',
            harvestYear: 2024,
            cuppingScore: 88,
            isActive: true
        },

        // Costa Rica (3 products)
        {
            sku: 'CRI-TAR-250',
            categoryId: coffeeBeans.id,
            subcategoryId: costaRica.id,
            name: 'Costa Rica Tarrazú',
            slug: 'costa-rica-tarrazu',
            description: 'Clean and bright with citrus and honey notes.',
            priceCents: 2699,
            stock: 40,
            unit: 'bag',
            weight: 250,
            origin: 'costa_rica',
            roastLevel: 'medium',
            processingMethod: 'washed',
            altitude: 1700,
            varietals: 'Caturra, Catuaí',
            harvestYear: 2024,
            cuppingScore: 86,
            isActive: true
        },
        {
            sku: 'CRI-WES-250',
            categoryId: coffeeBeans.id,
            subcategoryId: costaRica.id,
            name: 'Costa Rica West Valley',
            slug: 'costa-rica-west-valley',
            description: 'Sweet and balanced with chocolate and fruit notes.',
            priceCents: 2599,
            stock: 35,
            unit: 'bag',
            weight: 250,
            origin: 'costa_rica',
            roastLevel: 'medium',
            processingMethod: 'honey',
            altitude: 1600,
            varietals: 'Caturra',
            harvestYear: 2024,
            cuppingScore: 85,
            isActive: true
        },
        {
            sku: 'CRI-CEN-500',
            categoryId: coffeeBeans.id,
            subcategoryId: costaRica.id,
            name: 'Costa Rica Central Valley',
            slug: 'costa-rica-central-valley',
            description: 'Well-balanced with apple and caramel notes.',
            priceCents: 4999,
            stock: 30,
            unit: 'bag',
            weight: 500,
            origin: 'costa_rica',
            roastLevel: 'light',
            processingMethod: 'washed',
            altitude: 1500,
            varietals: 'Caturra, Catuaí',
            harvestYear: 2024,
            cuppingScore: 84,
            isActive: true
        },

        // Blend (2 products)
        {
            sku: 'BLD-HOU-250',
            categoryId: coffeeBeans.id,
            subcategoryId: blend.id,
            name: 'House Blend',
            slug: 'house-blend',
            description: 'Balanced blend of South American and African beans.',
            priceCents: 2199,
            stock: 80,
            unit: 'bag',
            weight: 250,
            origin: 'other',
            roastLevel: 'medium',
            processingMethod: 'washed',
            harvestYear: 2024,
            cuppingScore: 83,
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'BLD-ESP-250',
            categoryId: coffeeBeans.id,
            subcategoryId: blend.id,
            name: 'Espresso Blend',
            slug: 'espresso-blend',
            description: 'Rich and bold blend perfect for espresso.',
            priceCents: 2399,
            stock: 70,
            unit: 'bag',
            weight: 250,
            origin: 'other',
            roastLevel: 'dark',
            processingMethod: 'natural',
            harvestYear: 2024,
            cuppingScore: 84,
            isFeatured: true,
            isActive: true
        },

        // ========== Brewing Gear (15 products) ==========

        // Kettles (3 products)
        {
            sku: 'KET-FEL-STA',
            categoryId: brewingGear.id,
            subcategoryId: kettles.id,
            name: 'Fellow Stagg EKG Electric Kettle',
            slug: 'fellow-stagg-ekg-electric-kettle',
            description: 'Precision pour-over kettle with variable temperature control.',
            priceCents: 19900,
            stock: 25,
            unit: 'piece',
            brand: 'Fellow',
            capacity: 900,
            material: 'Stainless Steel',
            color: 'Matte Black',
            specifications: 'Temperature range: 135°F-212°F, 1200W',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'KET-HAR-BUO',
            categoryId: brewingGear.id,
            subcategoryId: kettles.id,
            name: 'Hario V60 Buono Kettle',
            slug: 'hario-v60-buono-kettle',
            description: 'Classic gooseneck kettle for pour-over brewing.',
            priceCents: 5900,
            stock: 40,
            unit: 'piece',
            brand: 'Hario',
            capacity: 1200,
            material: 'Stainless Steel',
            specifications: 'Stovetop compatible',
            isActive: true
        },
        {
            sku: 'KET-BOD-GOO',
            categoryId: brewingGear.id,
            subcategoryId: kettles.id,
            name: 'Bodum Gooseneck Kettle',
            slug: 'bodum-gooseneck-kettle',
            description: 'Affordable gooseneck kettle for precise pouring.',
            priceCents: 3900,
            stock: 35,
            unit: 'piece',
            brand: 'Bodum',
            capacity: 1000,
            material: 'Stainless Steel',
            specifications: 'Stovetop compatible',
            isActive: true
        },

        // Drippers (4 products)
        {
            sku: 'DRI-HAR-V60',
            categoryId: brewingGear.id,
            subcategoryId: drippers.id,
            name: 'Hario V60 Ceramic Dripper',
            slug: 'hario-v60-ceramic-dripper',
            description: 'Iconic cone-shaped dripper for clean, bright coffee.',
            priceCents: 2900,
            stock: 60,
            unit: 'piece',
            brand: 'Hario',
            size: '02',
            material: 'Ceramic',
            color: 'White',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'DRI-KAL-WAV',
            categoryId: brewingGear.id,
            subcategoryId: drippers.id,
            name: 'Kalita Wave 185',
            slug: 'kalita-wave-185',
            description: 'Flat-bottom dripper for consistent extraction.',
            priceCents: 3500,
            stock: 45,
            unit: 'piece',
            brand: 'Kalita',
            size: '185',
            material: 'Stainless Steel',
            isActive: true
        },
        {
            sku: 'DRI-CHE-CLA',
            categoryId: brewingGear.id,
            subcategoryId: drippers.id,
            name: 'Chemex Classic 6-Cup',
            slug: 'chemex-classic-6-cup',
            description: 'Elegant glass pour-over brewer with wooden collar.',
            priceCents: 4900,
            stock: 35,
            unit: 'piece',
            brand: 'Chemex',
            capacity: 900,
            material: 'Borosilicate Glass',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'DRI-ORI-DRI',
            categoryId: brewingGear.id,
            subcategoryId: drippers.id,
            name: 'Origami Dripper',
            slug: 'origami-dripper',
            description: 'Versatile dripper compatible with multiple filter types.',
            priceCents: 3900,
            stock: 30,
            unit: 'piece',
            brand: 'Origami',
            size: 'Medium',
            material: 'Porcelain',
            color: 'White',
            isActive: true
        },

        // Grinders (4 products)
        {
            sku: 'GRI-BAR-ENC',
            categoryId: brewingGear.id,
            subcategoryId: grinders.id,
            name: 'Baratza Encore Grinder',
            slug: 'baratza-encore-grinder',
            description: 'Entry-level burr grinder with 40 grind settings.',
            priceCents: 16900,
            stock: 20,
            unit: 'piece',
            brand: 'Baratza',
            specifications: '40 grind settings, 8oz bean hopper',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'GRI-1ZP-JX',
            categoryId: brewingGear.id,
            subcategoryId: grinders.id,
            name: '1Zpresso JX Manual Grinder',
            slug: '1zpresso-jx-manual-grinder',
            description: 'Premium manual grinder with 48mm conical burrs.',
            priceCents: 13900,
            stock: 25,
            unit: 'piece',
            brand: '1Zpresso',
            specifications: '48mm conical burrs, 35g capacity',
            isActive: true
        },
        {
            sku: 'GRI-HAR-SKE',
            categoryId: brewingGear.id,
            subcategoryId: grinders.id,
            name: 'Hario Skerton Pro',
            slug: 'hario-skerton-pro',
            description: 'Affordable manual grinder with ceramic burrs.',
            priceCents: 5900,
            stock: 40,
            unit: 'piece',
            brand: 'Hario',
            specifications: 'Ceramic conical burrs, 100g capacity',
            isActive: true
        },
        {
            sku: 'GRI-FEL-ODE',
            categoryId: brewingGear.id,
            subcategoryId: grinders.id,
            name: 'Fellow Ode Brew Grinder',
            slug: 'fellow-ode-brew-grinder',
            description: 'Flat burr grinder designed for filter coffee.',
            priceCents: 32900,
            stock: 15,
            unit: 'piece',
            brand: 'Fellow',
            specifications: '64mm flat burrs, 31 grind settings',
            isFeatured: true,
            isActive: true
        },

        // French Press (2 products)
        {
            sku: 'FRE-BOD-CHA',
            categoryId: brewingGear.id,
            subcategoryId: frenchPress.id,
            name: 'Bodum Chambord French Press',
            slug: 'bodum-chambord-french-press',
            description: 'Classic French press with chrome-plated frame.',
            priceCents: 4900,
            stock: 35,
            unit: 'piece',
            brand: 'Bodum',
            capacity: 1000,
            material: 'Borosilicate Glass',
            isActive: true
        },
        {
            sku: 'FRE-ESP-P7',
            categoryId: brewingGear.id,
            subcategoryId: frenchPress.id,
            name: 'Espro P7 French Press',
            slug: 'espro-p7-french-press',
            description: 'Double micro-filter French press for clean coffee.',
            priceCents: 9900,
            stock: 25,
            unit: 'piece',
            brand: 'Espro',
            capacity: 950,
            material: 'Stainless Steel',
            isFeatured: true,
            isActive: true
        },

        // Espresso Machines (2 products)
        {
            sku: 'ESP-BRE-BAM',
            categoryId: brewingGear.id,
            subcategoryId: espresso.id,
            name: 'Breville Bambino Plus',
            slug: 'breville-bambino-plus',
            description: 'Compact espresso machine with automatic milk frother.',
            priceCents: 49900,
            stock: 10,
            unit: 'piece',
            brand: 'Breville',
            specifications: '15 bar pump, 3 second heat-up',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'ESP-GAG-CLA',
            categoryId: brewingGear.id,
            subcategoryId: espresso.id,
            name: 'Gaggia Classic Pro',
            slug: 'gaggia-classic-pro',
            description: 'Semi-automatic espresso machine with commercial portafilter.',
            priceCents: 54900,
            stock: 8,
            unit: 'piece',
            brand: 'Gaggia',
            specifications: '15 bar pump, 58mm portafilter',
            isFeatured: true,
            isActive: true
        },

        // ========== Accessories (10 products) ==========

        // Cups & Mugs (3 products)
        {
            sku: 'CUP-KEE-12',
            categoryId: accessories.id,
            subcategoryId: cupsMugs.id,
            name: 'KeepCup Brew 12oz',
            slug: 'keepcup-brew-12oz',
            description: 'Reusable glass coffee cup with cork band.',
            priceCents: 2900,
            stock: 50,
            unit: 'piece',
            brand: 'KeepCup',
            capacity: 340,
            material: 'Tempered Glass',
            color: 'Cork',
            isActive: true
        },
        {
            sku: 'CUP-FEL-CAR',
            categoryId: accessories.id,
            subcategoryId: cupsMugs.id,
            name: 'Fellow Carter Move Mug',
            slug: 'fellow-carter-move-mug',
            description: 'Insulated travel mug with ceramic coating.',
            priceCents: 3900,
            stock: 40,
            unit: 'piece',
            brand: 'Fellow',
            capacity: 473,
            material: 'Stainless Steel',
            color: 'Matte Black',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'CUP-NOT-CER',
            categoryId: accessories.id,
            subcategoryId: cupsMugs.id,
            name: 'NotNeutral Lino Mug',
            slug: 'notneutral-lino-mug',
            description: 'Porcelain mug designed for latte art.',
            priceCents: 1900,
            stock: 60,
            unit: 'piece',
            brand: 'NotNeutral',
            capacity: 296,
            material: 'Porcelain',
            color: 'White',
            isActive: true
        },

        // Storage (3 products)
        {
            sku: 'STO-AIR-VAC',
            categoryId: accessories.id,
            subcategoryId: storage.id,
            name: 'Airscape Coffee Canister',
            slug: 'airscape-coffee-canister',
            description: 'Vacuum-sealed canister to keep coffee fresh.',
            priceCents: 3900,
            stock: 35,
            unit: 'piece',
            brand: 'Airscape',
            capacity: 500,
            material: 'Stainless Steel',
            specifications: 'Holds 1lb of coffee beans',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'STO-FEL-ATC',
            categoryId: accessories.id,
            subcategoryId: storage.id,
            name: 'Fellow Atmos Vacuum Canister',
            slug: 'fellow-atmos-vacuum-canister',
            description: 'Twist-to-lock vacuum canister with date tracker.',
            priceCents: 3500,
            stock: 30,
            unit: 'piece',
            brand: 'Fellow',
            capacity: 400,
            material: 'Borosilicate Glass',
            specifications: 'Integrated date tracker',
            isActive: true
        },
        {
            sku: 'STO-COF-VAU',
            categoryId: accessories.id,
            subcategoryId: storage.id,
            name: 'Coffee Gator Stainless Canister',
            slug: 'coffee-gator-stainless-canister',
            description: 'Airtight canister with CO2 release valve.',
            priceCents: 2900,
            stock: 40,
            unit: 'piece',
            brand: 'Coffee Gator',
            capacity: 500,
            material: 'Stainless Steel',
            specifications: 'Built-in date tracker',
            isActive: true
        },

        // Scales (2 products)
        {
            sku: 'SCA-HAR-DRI',
            categoryId: accessories.id,
            subcategoryId: scales.id,
            name: 'Hario V60 Drip Scale',
            slug: 'hario-v60-drip-scale',
            description: 'Digital scale with built-in timer for pour-over.',
            priceCents: 5900,
            stock: 30,
            unit: 'piece',
            brand: 'Hario',
            specifications: '2kg capacity, 0.1g precision, timer',
            isFeatured: true,
            isActive: true
        },
        {
            sku: 'SCA-ACE-PEA',
            categoryId: accessories.id,
            subcategoryId: scales.id,
            name: 'Acaia Pearl Coffee Scale',
            slug: 'acaia-pearl-coffee-scale',
            description: 'Professional coffee scale with Bluetooth connectivity.',
            priceCents: 24900,
            stock: 15,
            unit: 'piece',
            brand: 'Acaia',
            specifications: '2kg capacity, 0.1g precision, Bluetooth',
            isFeatured: true,
            isActive: true
        },

        // Filters (2 products)
        {
            sku: 'FIL-HAR-V60',
            categoryId: accessories.id,
            subcategoryId: filters.id,
            name: 'Hario V60 Paper Filters (100 pack)',
            slug: 'hario-v60-paper-filters-100-pack',
            description: 'Oxygen-bleached paper filters for V60 dripper.',
            priceCents: 1200,
            stock: 100,
            unit: 'box',
            brand: 'Hario',
            size: '02',
            specifications: '100 filters per pack',
            isActive: true
        },
        {
            sku: 'FIL-CHE-SQU',
            categoryId: accessories.id,
            subcategoryId: filters.id,
            name: 'Chemex Square Filters (100 pack)',
            slug: 'chemex-square-filters-100-pack',
            description: 'Bonded paper filters for Chemex brewers.',
            priceCents: 1500,
            stock: 80,
            unit: 'box',
            brand: 'Chemex',
            specifications: '100 filters per pack',
            isActive: true
        }
    ]).returning();

    console.log(`✅ Created ${coffeeProducts.length} products`);

    // ==================== 4. 添加风味标签 ====================
    console.log('🏷️  Adding flavor notes...');

    const flavorNotesData: Array<{ productId: number; flavorNote: 'floral' | 'fruity' | 'chocolate' | 'nutty' | 'caramel' | 'citrus' | 'berry' | 'spicy' | 'earthy' | 'sweet' }> = [
        // Ethiopian Yirgacheffe
        { productId: coffeeProducts[0].id, flavorNote: 'floral' },
        { productId: coffeeProducts[0].id, flavorNote: 'citrus' },
        { productId: coffeeProducts[0].id, flavorNote: 'fruity' },

        // Ethiopian Sidamo
        { productId: coffeeProducts[1].id, flavorNote: 'fruity' },
        { productId: coffeeProducts[1].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[1].id, flavorNote: 'berry' },

        // Ethiopian Harrar
        { productId: coffeeProducts[2].id, flavorNote: 'fruity' },
        { productId: coffeeProducts[2].id, flavorNote: 'berry' },

        // Ethiopian Guji
        { productId: coffeeProducts[3].id, flavorNote: 'floral' },
        { productId: coffeeProducts[3].id, flavorNote: 'fruity' },
        { productId: coffeeProducts[3].id, flavorNote: 'citrus' },

        // Ethiopian Limu
        { productId: coffeeProducts[4].id, flavorNote: 'citrus' },
        { productId: coffeeProducts[4].id, flavorNote: 'sweet' },

        // Colombia Supremo
        { productId: coffeeProducts[5].id, flavorNote: 'caramel' },
        { productId: coffeeProducts[5].id, flavorNote: 'nutty' },

        // Colombia Huila
        { productId: coffeeProducts[6].id, flavorNote: 'fruity' },
        { productId: coffeeProducts[6].id, flavorNote: 'sweet' },
        { productId: coffeeProducts[6].id, flavorNote: 'caramel' },

        // Colombia Nariño
        { productId: coffeeProducts[7].id, flavorNote: 'citrus' },
        { productId: coffeeProducts[7].id, flavorNote: 'chocolate' },

        // Colombia Tolima
        { productId: coffeeProducts[8].id, flavorNote: 'floral' },
        { productId: coffeeProducts[8].id, flavorNote: 'fruity' },

        // Guatemala Antigua
        { productId: coffeeProducts[10].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[10].id, flavorNote: 'spicy' },

        // Guatemala Huehuetenango
        { productId: coffeeProducts[11].id, flavorNote: 'fruity' },
        { productId: coffeeProducts[11].id, flavorNote: 'citrus' },

        // Guatemala Atitlán
        { productId: coffeeProducts[12].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[12].id, flavorNote: 'floral' },

        // Brazil Santos
        { productId: coffeeProducts[14].id, flavorNote: 'nutty' },
        { productId: coffeeProducts[14].id, flavorNote: 'sweet' },

        // Brazil Cerrado
        { productId: coffeeProducts[15].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[15].id, flavorNote: 'sweet' },

        // Kenya AA
        { productId: coffeeProducts[17].id, flavorNote: 'berry' },
        { productId: coffeeProducts[17].id, flavorNote: 'citrus' },

        // Kenya Nyeri
        { productId: coffeeProducts[18].id, flavorNote: 'berry' },
        { productId: coffeeProducts[18].id, flavorNote: 'fruity' },

        // Costa Rica Tarrazú
        { productId: coffeeProducts[20].id, flavorNote: 'citrus' },
        { productId: coffeeProducts[20].id, flavorNote: 'sweet' },

        // House Blend
        { productId: coffeeProducts[23].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[23].id, flavorNote: 'nutty' },
        { productId: coffeeProducts[23].id, flavorNote: 'caramel' },

        // Espresso Blend
        { productId: coffeeProducts[24].id, flavorNote: 'chocolate' },
        { productId: coffeeProducts[24].id, flavorNote: 'caramel' },
    ];

    await db.insert(productFlavorNotes).values(flavorNotesData);
    console.log(`✅ Added ${flavorNotesData.length} flavor notes`);

    // ==================== 5. 添加商品标签 ====================
    console.log('🏷️  Adding product tags...');

    const tagsData: Array<{ productId: number; tag: 'organic' | 'limited_offer' | 'new_arrival' | 'best_seller' | 'seasonal' }> = [
        // Featured products
        { productId: coffeeProducts[0].id, tag: 'new_arrival' },
        { productId: coffeeProducts[3].id, tag: 'organic' },
        { productId: coffeeProducts[6].id, tag: 'best_seller' },
        { productId: coffeeProducts[17].id, tag: 'organic' },
        { productId: coffeeProducts[18].id, tag: 'limited_offer' },
        { productId: coffeeProducts[23].id, tag: 'best_seller' },
        { productId: coffeeProducts[24].id, tag: 'best_seller' },

        // Brewing gear
        { productId: coffeeProducts[25].id, tag: 'new_arrival' },
        { productId: coffeeProducts[28].id, tag: 'best_seller' },
        { productId: coffeeProducts[31].id, tag: 'best_seller' },
        { productId: coffeeProducts[32].id, tag: 'new_arrival' },

        // Accessories
        { productId: coffeeProducts[42].id, tag: 'best_seller' },
        { productId: coffeeProducts[44].id, tag: 'new_arrival' },
    ];

    await db.insert(productTags).values(tagsData);
    console.log(`✅ Added ${tagsData.length} product tags`);

    console.log('🎉 Seed completed successfully!');
}

seed()
    .catch((error) => {
        console.error('❌ Seed failed:', error);
        process.exit(1);
    })
    .finally(() => {
        process.exit(0);
    });

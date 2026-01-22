import { pgTable, foreignKey, serial, integer, timestamp, varchar, boolean, unique, text, uuid, date, index, uniqueIndex, pgEnum, json } from "drizzle-orm/pg-core"
import { sql } from "drizzle-orm"

// ==================== 枚举类型定义 ====================

// 用户相关
export const loginProvider = pgEnum("login_provider", ['google', 'apple', 'email'])
export const addressType = pgEnum("address_type", ['shipping', 'billing'])

// 订单相关
export const orderStatus = pgEnum("order_status", [
	'pending_payment',
	'payment_failed',
	'paid',
	'processing',
	'awaiting_shipment',
	'shipped',
	'in_transit',
	'out_for_delivery',
	'delivered',
	'cancelled',
	'refund_requested',
	'refunded'
])
export const paymentStatus = pgEnum("payment_status", ['pending', 'completed', 'failed', 'refunded'])
export const paymentMethod = pgEnum("payment_method", ['credit_card', 'debit_card', 'paypal', 'stripe'])

// ==================== 咖啡专用枚举 ====================

// 烘焙度
export const roastLevel = pgEnum("roast_level", ['light', 'medium', 'dark'])

// 风味标签
export const flavorNote = pgEnum("flavor_note", [
	'floral',
	'fruity',
	'chocolate',
	'nutty',
	'caramel',
	'citrus',
	'berry',
	'spicy',
	'earthy',
	'sweet'
])

// 咖啡豆产地
export const coffeeOrigin = pgEnum("coffee_origin", [
	'ethiopia',
	'colombia',
	'guatemala',
	'brazil',
	'kenya',
	'costa_rica',
	'indonesia',
	'yemen',
	'peru',
	'honduras',
	'other'
])

// 处理方式
export const processingMethod = pgEnum("processing_method", [
	'washed',
	'natural',
	'honey',
	'anaerobic'
])

// 商品标签
export const productTag = pgEnum("product_tag", [
	'organic',
	'limited_offer',
	'new_arrival',
	'best_seller',
	'seasonal'
])

// ==================== 用户系统 ====================

export const users = pgTable("users", {
	id: uuid().defaultRandom().primaryKey().notNull(),
	email: varchar({ length: 255 }).notNull(),
	password: varchar({ length: 255 }),
	firstName: varchar("first_name", { length: 100 }),
	lastName: varchar("last_name", { length: 100 }),
	displayName: varchar("display_name", { length: 120 }),
	deletedAt: timestamp("deleted_at", { withTimezone: true, mode: 'string' }),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	unique("users_email_unique").on(table.email),
	index("users_email_idx").on(table.email),
	index("users_deleted_at_idx").on(table.deletedAt),
]);

export const accounts = pgTable("accounts", {
	id: uuid().defaultRandom().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	provider: loginProvider().notNull(),
	providerAccountId: varchar("provider_account_id", { length: 255 }).notNull(),
	emailAtLink: varchar("email_at_link", { length: 255 }),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "accounts_user_id_fkey"
	}).onDelete("cascade"),
	uniqueIndex("accounts_provider_account_unique").on(table.provider, table.providerAccountId),
	uniqueIndex("accounts_user_provider_unique").on(table.userId, table.provider),
]);

export const userProfiles = pgTable("user_profiles", {
	id: serial().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	nickname: varchar({ length: 50 }),
	avatarUrl: text("avatar_url"),
	gender: varchar({ length: 10 }),
	birthDate: date("birth_date", { mode: 'string' }),
	phone: varchar({ length: 20 }),
	bio: text(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "user_profiles_user_id_fkey"
	}).onDelete("cascade"),
	unique("user_profiles_user_id_unique").on(table.userId),
]);

// ==================== 地址管理 ====================

export const addresses = pgTable("addresses", {
	id: uuid().defaultRandom().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	type: addressType().notNull(),
	isDefault: boolean("is_default").default(false).notNull(),
	recipientName: varchar("recipient_name", { length: 120 }).notNull(),
	phone: varchar({ length: 20 }).notNull(),
	province: varchar({ length: 50 }).notNull(),
	city: varchar({ length: 50 }).notNull(),
	district: varchar({ length: 50 }).notNull(),
	addressText: text("address_text").notNull(),
	postalCode: varchar("postal_code", { length: 20 }),
	countryCode: varchar("country_code", { length: 2 }).default('AU').notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "addresses_user_id_fkey"
	}).onDelete("cascade"),
	uniqueIndex("addresses_default_per_type_unique")
		.on(table.userId, table.type)
		.where(sql`is_default = true`),
	index("addresses_user_id_idx").on(table.userId),
]);

// ==================== 商品系统 ====================

export const categories = pgTable("categories", {
	id: serial().primaryKey().notNull(),
	slug: varchar({ length: 50 }).notNull(),
	name: varchar({ length: 100 }).notNull(),
	description: text(),
	imageUrl: text("image_url"),
	displayOrder: integer("display_order").default(0).notNull(),
	isActive: boolean("is_active").default(true).notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	unique("categories_slug_unique").on(table.slug),
]);

export const subcategories = pgTable("subcategories", {
	id: serial().primaryKey().notNull(),
	categoryId: integer("category_id").notNull(),
	slug: varchar({ length: 50 }).notNull(),
	name: varchar({ length: 100 }).notNull(),
	description: text(),
	displayOrder: integer("display_order").default(0).notNull(),
	isActive: boolean("is_active").default(true).notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.categoryId],
		foreignColumns: [categories.id],
		name: "subcategories_category_id_fkey"
	}).onDelete("cascade"),
	unique("subcategories_slug_unique").on(table.slug),
	index("subcategories_category_id_idx").on(table.categoryId),
]);

export const products = pgTable("products", {
	id: serial().primaryKey().notNull(),
	uuid: uuid().defaultRandom().notNull(),
	sku: varchar({ length: 50 }).notNull(),
	categoryId: integer("category_id").notNull(),
	subcategoryId: integer("subcategory_id"),
	name: varchar({ length: 255 }).notNull(),
	slug: varchar({ length: 255 }).notNull(),
	description: text(),

	// 价格 - 使用分(cents)
	priceCents: integer("price_cents").notNull(),
	currency: varchar({ length: 10 }).default('AUD').notNull(),

	// 库存
	stock: integer().notNull().default(0),
	unit: varchar({ length: 20 }).notNull().default('bag'),

	// 通用属性
	brand: varchar({ length: 100 }),
	weight: integer(),

	// 咖啡豆专用字段
	origin: coffeeOrigin(),
	roastLevel: roastLevel("roast_level"),
	processingMethod: processingMethod("processing_method"),
	altitude: integer(),
	varietals: varchar({ length: 255 }),
	harvestYear: integer("harvest_year"),
	cuppingScore: integer("cupping_score"),

	// 器具/配件专用字段
	material: varchar({ length: 100 }),
	color: varchar({ length: 50 }),
	size: varchar({ length: 50 }),
	capacity: integer(),
	specifications: text(),

	// 状态
	isFeatured: boolean("is_featured").default(false).notNull(),
	isActive: boolean("is_active").default(true).notNull(),

	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.categoryId],
		foreignColumns: [categories.id],
		name: "products_category_id_fkey"
	}),
	foreignKey({
		columns: [table.subcategoryId],
		foreignColumns: [subcategories.id],
		name: "products_subcategory_id_fkey"
	}),
	unique("products_sku_unique").on(table.sku),
	unique("products_uuid_unique").on(table.uuid),
	unique("products_slug_unique").on(table.slug),
	index("products_category_id_idx").on(table.categoryId),
	index("products_subcategory_id_idx").on(table.subcategoryId),
	index("products_is_active_idx").on(table.isActive),
	index("products_is_featured_idx").on(table.isFeatured),
	index("products_origin_idx").on(table.origin),
	index("products_roast_level_idx").on(table.roastLevel),
]);

export const productFlavorNotes = pgTable("product_flavor_notes", {
	id: serial().primaryKey().notNull(),
	productId: integer("product_id").notNull(),
	flavorNote: flavorNote("flavor_note").notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "product_flavor_notes_product_id_fkey"
	}).onDelete("cascade"),
	unique("product_flavor_notes_unique").on(table.productId, table.flavorNote),
	index("product_flavor_notes_product_id_idx").on(table.productId),
	index("product_flavor_notes_flavor_note_idx").on(table.flavorNote),
]);

export const productTags = pgTable("product_tags", {
	id: serial().primaryKey().notNull(),
	productId: integer("product_id").notNull(),
	tag: productTag().notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "product_tags_product_id_fkey"
	}).onDelete("cascade"),
	unique("product_tags_unique").on(table.productId, table.tag),
	index("product_tags_product_id_idx").on(table.productId),
	index("product_tags_tag_idx").on(table.tag),
]);

export const productImages = pgTable("product_images", {
	id: serial().primaryKey().notNull(),
	productId: integer("product_id").notNull(),
	imageUrl: text("image_url").notNull(),
	isPrimary: boolean("is_primary").default(false).notNull(),
	displayOrder: integer("display_order").default(0).notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "product_images_product_id_fkey"
	}).onDelete("cascade"),
	index("product_images_product_id_idx").on(table.productId),
]);

// ==================== 购物功能 ====================

export const cartItems = pgTable("cart_items", {
	id: serial().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	productId: integer("product_id").notNull(),
	quantity: integer().notNull().default(1),
	addedAt: timestamp("added_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "cart_items_user_id_fkey"
	}).onDelete("cascade"),
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "cart_items_product_id_fkey"
	}).onDelete("cascade"),
	unique("cart_items_user_product_unique").on(table.userId, table.productId),
	index("cart_items_user_id_idx").on(table.userId),
]);

export const wishlistItems = pgTable("wishlist_items", {
	id: serial().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	productId: integer("product_id").notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "wishlist_items_user_id_fkey"
	}).onDelete("cascade"),
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "wishlist_items_product_id_fkey"
	}).onDelete("cascade"),
	unique("wishlist_items_user_product_unique").on(table.userId, table.productId),
	index("wishlist_items_user_id_idx").on(table.userId),
]);

// ==================== 结账流程 ====================

export const checkoutIntents = pgTable("checkout_intents", {
	id: serial().primaryKey().notNull(),
	uuid: uuid().defaultRandom().notNull(),
	userId: uuid("user_id").notNull(),

	// 选中的商品快照
	selectedItems: json("selected_items").notNull(),

	// 用户填写的信息
	shippingAddress: json("shipping_address"),
	billingAddress: json("billing_address"),
	shippingMethod: varchar("shipping_method", { length: 50 }),

	// 金额
	subtotalCents: integer("subtotal_cents").notNull(),
	shippingFeeCents: integer("shipping_fee_cents").default(0).notNull(),
	taxCents: integer("tax_cents").default(0).notNull(),
	grandTotalCents: integer("grand_total_cents").notNull(),
	currency: varchar({ length: 10 }).default('AUD').notNull(),

	// 状态
	completedOrderId: integer("completed_order_id"),

	// 过期管理
	expiresAt: timestamp("expires_at", { withTimezone: true, mode: 'string' }).notNull(),

	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "checkout_intents_user_id_fkey"
	}),
	foreignKey({
		columns: [table.completedOrderId],
		foreignColumns: [orders.id],
		name: "checkout_intents_completed_order_id_fkey"
	}),
	unique("checkout_intents_uuid_unique").on(table.uuid),
	index("checkout_intents_user_id_idx").on(table.userId),
	index("checkout_intents_expires_at_idx").on(table.expiresAt),
]);

// ==================== 订单系统 ====================

export const orders = pgTable("orders", {
	id: serial().primaryKey().notNull(),
	uuid: uuid().defaultRandom().notNull(),
	orderNumber: varchar("order_number", { length: 50 }).notNull(),
	userId: uuid("user_id").notNull(),
	email: varchar({ length: 255 }).notNull(),

	// 地址快照
	shippingAddress: json("shipping_address").notNull(),
	billingAddress: json("billing_address"),

	// 金额
	subtotalCents: integer("subtotal_cents").notNull().default(0),
	shippingFeeCents: integer("shipping_fee_cents").notNull().default(0),
	taxCents: integer("tax_cents").notNull().default(0),
	grandTotalCents: integer("grand_total_cents").notNull().default(0),
	currency: varchar({ length: 10 }).default('AUD').notNull(),

	// 状态
	status: orderStatus().default('pending_payment').notNull(),

	// 支付信息
	stripeSessionId: varchar("stripe_session_id", { length: 255 }),
	stripePaymentIntentId: varchar("stripe_payment_intent_id", { length: 255 }),
	paidAt: timestamp("paid_at", { withTimezone: true, mode: 'string' }),

	// 发货信息
	shippedAt: timestamp("shipped_at", { withTimezone: true, mode: 'string' }),
	deliveredAt: timestamp("delivered_at", { withTimezone: true, mode: 'string' }),

	// 订单备注
	notes: text(),

	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.userId],
		foreignColumns: [users.id],
		name: "orders_user_id_fkey"
	}),
	unique("orders_uuid_unique").on(table.uuid),
	unique("orders_order_number_unique").on(table.orderNumber),
	index("orders_user_id_idx").on(table.userId),
	index("orders_status_idx").on(table.status),
	index("orders_created_at_idx").on(table.createdAt),
]);

export const orderItems = pgTable("order_items", {
	id: serial().primaryKey().notNull(),
	orderId: integer("order_id").notNull(),
	productId: integer("product_id").notNull(),

	// 商品快照
	productName: varchar("product_name", { length: 255 }).notNull(),
	productSku: varchar("product_sku", { length: 50 }).notNull(),
	productSlug: varchar("product_slug", { length: 255 }),
	imageUrl: text("image_url"),

	// 价格和数量
	unitPriceCents: integer("unit_price_cents").notNull(),
	quantity: integer().notNull(),
	lineTotalCents: integer("line_total_cents").notNull(),
	currency: varchar({ length: 10 }).default('AUD').notNull(),

	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.orderId],
		foreignColumns: [orders.id],
		name: "order_items_order_id_fkey"
	}).onDelete("cascade"),
	foreignKey({
		columns: [table.productId],
		foreignColumns: [products.id],
		name: "order_items_product_id_fkey"
	}),
	index("order_items_order_id_idx").on(table.orderId),
]);

// ==================== 支付系统 ====================

export const payments = pgTable("payments", {
	id: serial().primaryKey().notNull(),
	uuid: uuid().defaultRandom().notNull(),
	orderId: integer("order_id").notNull(),
	paymentMethod: paymentMethod("payment_method").notNull(),
	transactionId: varchar("transaction_id", { length: 100 }),
	amountCents: integer("amount_cents").notNull(),
	currency: varchar({ length: 10 }).default('AUD').notNull(),
	status: paymentStatus().default('pending').notNull(),
	paidAt: timestamp("paid_at", { withTimezone: true, mode: 'string' }),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow().notNull(),
}, (table) => [
	foreignKey({
		columns: [table.orderId],
		foreignColumns: [orders.id],
		name: "payments_order_id_fkey"
	}),
	unique("payments_uuid_unique").on(table.uuid),
	unique("payments_order_id_unique").on(table.orderId),
	index("payments_status_idx").on(table.status),
]);

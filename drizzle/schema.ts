import { pgTable, foreignKey, serial, integer, timestamp, varchar, boolean, unique, text, numeric, uuid } from "drizzle-orm/pg-core"
import { sql } from "drizzle-orm"



export const cafe1316Cart = pgTable("cafe_1316_cart", {
	id: serial().primaryKey().notNull(),
	userId: uuid("user_id").notNull(),
	productId: uuid("product_id").notNull(),
	amount: integer().notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow(),
}, (table) => [
	foreignKey({
			columns: [table.userId],
			foreignColumns: [cafe1316Users.uuid],
			name: "cafe_1316_cart_user_id_fkey"
		}).onDelete("cascade"),
	foreignKey({
			columns: [table.productId],
			foreignColumns: [cafe1316Products.uuid],
			name: "cafe_1316_cart_product_id_fkey"
		}).onDelete("cascade"),
]);

export const cafe1316UserAddresses = pgTable("cafe1316_user_addresses", {
	id: serial().primaryKey().notNull(),
	userId: integer("user_id"),
	name: varchar({ length: 50 }).notNull(),
	receiver: varchar({ length: 50 }).notNull(),
	phone: varchar({ length: 20 }).notNull(),
	province: varchar({ length: 50 }).notNull(),
	city: varchar({ length: 50 }).notNull(),
	district: varchar({ length: 50 }).notNull(),
	detailedAddress: varchar("detailed_address", { length: 255 }).notNull(),
	isDefault: boolean("is_default").default(false),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow(),
}, (table) => [
	foreignKey({
			columns: [table.userId],
			foreignColumns: [cafe1316Users.id],
			name: "cafe1316_user_addresses_user_id_fkey"
		}).onDelete("cascade"),
]);

export const cafe1316ProductCategories = pgTable("cafe1316_product_categories", {
	id: serial().primaryKey().notNull(),
	name: varchar({ length: 100 }).notNull(),
	parentId: integer("parent_id"),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
}, (table) => [
	foreignKey({
			columns: [table.parentId],
			foreignColumns: [table.id],
			name: "cafe1316_product_categories_parent_id_fkey"
		}),
]);

export const cafe1316Products = pgTable("cafe1316_products", {
	id: serial("id").primaryKey().notNull(),
	uuid: uuid("uuid").notNull().default(sql`gen_random_uuid()`),
	sku: varchar({ length: 50 }).notNull(),
	productName: varchar("product_name", { length: 255 }).notNull(),
	origin: varchar({ length: 100 }),
	description: text(),
	roasting: varchar({ length: 50 }),
	material: varchar({ length: 100 }),
	brand: varchar({ length: 100 }),
	originalPrice: numeric("original_price", { precision: 10, scale:  2 }).notNull(),
	discountedPrice: numeric("discounted_price", { precision: 10, scale:  2 }),
	unit: varchar({ length: 50 }).notNull(),
	amount: integer().notNull(),
	productType: varchar("product_type", { length: 50 }).notNull(),
	categoryId: integer("category_id"),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	imageUrl: text("image_url"),
	isHot: boolean("is_hot").default(false),
	color: varchar({ length: 50 }),
	size: varchar({ length: 50 }),
	weight: numeric({ precision: 10, scale:  2 }),
	specifications: text(),
}, (table) => [
	foreignKey({
			columns: [table.categoryId],
			foreignColumns: [cafe1316ProductCategories.id],
			name: "cafe1316_products_category_id_fkey"
		}),
	unique("cafe1316_products_sku_key").on(table.sku),
	unique("cafe1316_products_uuid_key").on(table.uuid),
]);

export const cafe1316ProductImages = pgTable("cafe1316_product_images", {
	id: serial().primaryKey().notNull(),
	productId: integer("product_id"),
	imageUrl: text("image_url").notNull(),
	isPrimary: boolean("is_primary").default(false),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
}, (table) => [
	foreignKey({
			columns: [table.productId],
			foreignColumns: [cafe1316Products.id],
			name: "cafe1316_product_images_product_id_fkey"
		}).onDelete("cascade"),
]);

export const cafe1316Users = pgTable("cafe1316_users", {
	id: serial().primaryKey().notNull(),
	uuid: uuid().notNull(),
	username: varchar({ length: 50 }).notNull(),
	password: varchar({ length: 255 }).notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).default(sql`CURRENT_TIMESTAMP`),
	email: varchar({ length: 100 }).default('').notNull(),
}, (table) => [
	unique("cafe1316_users_uuid_key").on(table.uuid),
	unique("cafe1316_users_username_key").on(table.username),
]);

export const cafe1316Sessions = pgTable("cafe1316_sessions", {
	id: varchar({ length: 100 }).primaryKey().notNull(),
	userId: integer("user_id"),
	token: varchar({ length: 255 }).notNull(),
	expiresAt: timestamp("expires_at", { mode: 'string' }).notNull(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow(),
}, (table) => [
	foreignKey({
			columns: [table.userId],
			foreignColumns: [cafe1316Users.id],
			name: "cafe1316_sessions_user_id_fkey"
		}).onDelete("cascade"),
]);

export const cafe1316UserProfiles = pgTable("cafe1316_user_profiles", {
	id: serial().primaryKey().notNull(),
	userId: integer("user_id"),
	fullName: varchar("full_name", { length: 100 }),
	phone: varchar({ length: 20 }),
	avatarUrl: varchar("avatar_url", { length: 255 }),
	gender: varchar({ length: 10 }),
	birthDate: timestamp("birth_date", { mode: 'string' }),
	bio: text(),
	createdAt: timestamp("created_at", { withTimezone: true, mode: 'string' }).defaultNow(),
	updatedAt: timestamp("updated_at", { withTimezone: true, mode: 'string' }).defaultNow(),
}, (table) => [
	foreignKey({
			columns: [table.userId],
			foreignColumns: [cafe1316Users.id],
			name: "cafe1316_user_profiles_user_id_fkey"
		}).onDelete("cascade"),
]);

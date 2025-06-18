import { pgTable, foreignKey, serial, integer, text, boolean, timestamp, varchar, unique, numeric } from "drizzle-orm/pg-core"
import { sql } from "drizzle-orm"



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
	id: serial().primaryKey().notNull(),
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
]);

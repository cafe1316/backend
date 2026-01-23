import { relations } from "drizzle-orm/relations";
import {
	users,
	accounts,
	userProfiles,
	addresses,
	categories,
	subcategories,
	products,
	productFlavorNotes,
	productTags,
	productImages,
	cartItems,
	wishlistItems,
	checkoutIntents,
	orders,
	orderItems,
	payments
} from "./schema";

// ==================== 用户关系 ====================

export const usersRelations = relations(users, ({ one, many }) => ({
	profile: one(userProfiles, {
		fields: [users.id],
		references: [userProfiles.userId]
	}),
	accounts: many(accounts),
	addresses: many(addresses),
	cartItems: many(cartItems),
	wishlistItems: many(wishlistItems),
	checkoutIntents: many(checkoutIntents),
	orders: many(orders),
}));

export const accountsRelations = relations(accounts, ({ one }) => ({
	user: one(users, {
		fields: [accounts.userId],
		references: [users.id]
	}),
}));

export const userProfilesRelations = relations(userProfiles, ({ one }) => ({
	user: one(users, {
		fields: [userProfiles.userId],
		references: [users.id]
	}),
}));

// ==================== 地址关系 ====================

export const addressesRelations = relations(addresses, ({ one }) => ({
	user: one(users, {
		fields: [addresses.userId],
		references: [users.id]
	}),
}));

// ==================== 商品关系 ====================

export const categoriesRelations = relations(categories, ({ many }) => ({
	subcategories: many(subcategories),
	products: many(products),
}));

export const subcategoriesRelations = relations(subcategories, ({ one, many }) => ({
	category: one(categories, {
		fields: [subcategories.categoryId],
		references: [categories.id]
	}),
	products: many(products),
}));

export const productsRelations = relations(products, ({ one, many }) => ({
	category: one(categories, {
		fields: [products.categoryId],
		references: [categories.id]
	}),
	subcategory: one(subcategories, {
		fields: [products.subcategoryId],
		references: [subcategories.id]
	}),
	images: many(productImages),
	flavorNotes: many(productFlavorNotes),
	tags: many(productTags),
	cartItems: many(cartItems),
	wishlistItems: many(wishlistItems),
	orderItems: many(orderItems),
}));

export const productFlavorNotesRelations = relations(productFlavorNotes, ({ one }) => ({
	product: one(products, {
		fields: [productFlavorNotes.productId],
		references: [products.id]
	}),
}));

export const productTagsRelations = relations(productTags, ({ one }) => ({
	product: one(products, {
		fields: [productTags.productId],
		references: [products.id]
	}),
}));

export const productImagesRelations = relations(productImages, ({ one }) => ({
	product: one(products, {
		fields: [productImages.productId],
		references: [products.id]
	}),
}));

// ==================== 购物车和心愿单关系 ====================

export const cartItemsRelations = relations(cartItems, ({ one }) => ({
	user: one(users, {
		fields: [cartItems.userId],
		references: [users.id]
	}),
	product: one(products, {
		fields: [cartItems.productId],
		references: [products.id]
	}),
}));

export const wishlistItemsRelations = relations(wishlistItems, ({ one }) => ({
	user: one(users, {
		fields: [wishlistItems.userId],
		references: [users.id]
	}),
	product: one(products, {
		fields: [wishlistItems.productId],
		references: [products.id]
	}),
}));

// ==================== 结账流程关系 ====================

export const checkoutIntentsRelations = relations(checkoutIntents, ({ one }) => ({
	user: one(users, {
		fields: [checkoutIntents.userId],
		references: [users.id]
	}),
	completedOrder: one(orders, {
		fields: [checkoutIntents.completedOrderId],
		references: [orders.id]
	}),
}));

// ==================== 订单关系 ====================

export const ordersRelations = relations(orders, ({ one, many }) => ({
	user: one(users, {
		fields: [orders.userId],
		references: [users.id]
	}),
	items: many(orderItems),
	payment: one(payments, {
		fields: [orders.id],
		references: [payments.orderId]
	}),
}));

export const orderItemsRelations = relations(orderItems, ({ one }) => ({
	order: one(orders, {
		fields: [orderItems.orderId],
		references: [orders.id]
	}),
	product: one(products, {
		fields: [orderItems.productId],
		references: [products.id]
	}),
}));

// ==================== 支付关系 ====================

export const paymentsRelations = relations(payments, ({ one }) => ({
	order: one(orders, {
		fields: [payments.orderId],
		references: [orders.id]
	}),
}));
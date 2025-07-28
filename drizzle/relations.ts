import { relations } from "drizzle-orm/relations";
import { cafe1316Users, cafe1316ShoppingCarts, cafe1316Products, cafe1316UserAddresses, cafe1316ProductCategories, cafe1316ProductImages, cafe1316Sessions, cafe1316UserProfiles } from "./schema";

export const cafe1316ShoppingCartsRelations = relations(cafe1316ShoppingCarts, ({one}) => ({
	cafe1316User: one(cafe1316Users, {
		fields: [cafe1316ShoppingCarts.userId],
		references: [cafe1316Users.id]
	}),
	cafe1316Product: one(cafe1316Products, {
		fields: [cafe1316ShoppingCarts.productId],
		references: [cafe1316Products.id]
	}),
}));

export const cafe1316UsersRelations = relations(cafe1316Users, ({many}) => ({
	cafe1316ShoppingCarts: many(cafe1316ShoppingCarts),
	cafe1316UserAddresses: many(cafe1316UserAddresses),
	cafe1316Sessions: many(cafe1316Sessions),
	cafe1316UserProfiles: many(cafe1316UserProfiles),
}));

export const cafe1316ProductsRelations = relations(cafe1316Products, ({one, many}) => ({
	cafe1316ShoppingCarts: many(cafe1316ShoppingCarts),
	cafe1316ProductCategory: one(cafe1316ProductCategories, {
		fields: [cafe1316Products.categoryId],
		references: [cafe1316ProductCategories.id]
	}),
	cafe1316ProductImages: many(cafe1316ProductImages),
}));

export const cafe1316UserAddressesRelations = relations(cafe1316UserAddresses, ({one}) => ({
	cafe1316User: one(cafe1316Users, {
		fields: [cafe1316UserAddresses.userId],
		references: [cafe1316Users.id]
	}),
}));

export const cafe1316ProductCategoriesRelations = relations(cafe1316ProductCategories, ({one, many}) => ({
	cafe1316ProductCategory: one(cafe1316ProductCategories, {
		fields: [cafe1316ProductCategories.parentId],
		references: [cafe1316ProductCategories.id],
		relationName: "cafe1316ProductCategories_parentId_cafe1316ProductCategories_id"
	}),
	cafe1316ProductCategories: many(cafe1316ProductCategories, {
		relationName: "cafe1316ProductCategories_parentId_cafe1316ProductCategories_id"
	}),
	cafe1316Products: many(cafe1316Products),
}));

export const cafe1316ProductImagesRelations = relations(cafe1316ProductImages, ({one}) => ({
	cafe1316Product: one(cafe1316Products, {
		fields: [cafe1316ProductImages.productId],
		references: [cafe1316Products.id]
	}),
}));

export const cafe1316SessionsRelations = relations(cafe1316Sessions, ({one}) => ({
	cafe1316User: one(cafe1316Users, {
		fields: [cafe1316Sessions.userId],
		references: [cafe1316Users.id]
	}),
}));

export const cafe1316UserProfilesRelations = relations(cafe1316UserProfiles, ({one}) => ({
	cafe1316User: one(cafe1316Users, {
		fields: [cafe1316UserProfiles.userId],
		references: [cafe1316Users.id]
	}),
}));
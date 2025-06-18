import { relations } from "drizzle-orm/relations";
import { cafe1316Products, cafe1316ProductImages, cafe1316ProductCategories } from "./schema";

export const cafe1316ProductImagesRelations = relations(cafe1316ProductImages, ({one}) => ({
	cafe1316Product: one(cafe1316Products, {
		fields: [cafe1316ProductImages.productId],
		references: [cafe1316Products.id]
	}),
}));

export const cafe1316ProductsRelations = relations(cafe1316Products, ({one, many}) => ({
	cafe1316ProductImages: many(cafe1316ProductImages),
	cafe1316ProductCategory: one(cafe1316ProductCategories, {
		fields: [cafe1316Products.categoryId],
		references: [cafe1316ProductCategories.id]
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
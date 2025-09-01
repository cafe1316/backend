import db from "../db";
import { cafe1316Cart, cafe1316Products, cafe1316Users } from "../../drizzle/schema";
import { and,eq } from "drizzle-orm";

interface addCartProductParams{
    userId: string;
    productId: string;
    amount: number;
}

export const addCartProduct = async ({userId, productId, amount}: addCartProductParams): Promise<void> => {
    if(!userId || !productId || !amount){
        throw new Error("userId, productId, and amount are required");
    }
    const existing = await db.select().from(cafe1316Cart).where(and(
        eq(cafe1316Cart.userId, userId), 
        eq(cafe1316Cart.productId, productId)));

    if(existing.length > 0) {
        await db.update(cafe1316Cart).set({
            amount: amount,
            updatedAt: new Date().toISOString(),
        }).where(eq(cafe1316Cart.id, existing[0].id));
    }else{
        await db.insert(cafe1316Cart).values({
            userId,
            productId,
            amount,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
        });
    }
}

export const getCartItems = async (userId: string) => {
    const items = await db.select({
        cardId: cafe1316Cart.id,
        productId: cafe1316Products.uuid,
        amount: cafe1316Cart.amount,
        productName: cafe1316Products.productName,
        sku: cafe1316Products.sku,
        origin: cafe1316Products.origin,
        description: cafe1316Products.description,
        roasting: cafe1316Products.roasting,
        material: cafe1316Products.material,
        brand: cafe1316Products.brand,
        originalPrice: cafe1316Products.originalPrice,
        discountedPrice: cafe1316Products.discountedPrice,
        unit: cafe1316Products.unit,
        productType: cafe1316Products.productType,
        imageUrl: cafe1316Products.imageUrl,
        color: cafe1316Products.color,
        size: cafe1316Products.size,
        weight: cafe1316Products.weight,
        specifications: cafe1316Products.specifications,
    }).from(cafe1316Cart).where(eq(cafe1316Cart.userId, userId))
    .innerJoin(cafe1316Products, eq(cafe1316Cart.productId, cafe1316Products.uuid));

    return items;
}

export const updateCartItem = async(cartItemId:number, userId: string, amount:number) => {
    const existing = await db.select().from(cafe1316Cart).where(and(eq(cafe1316Cart.id, cartItemId), eq(cafe1316Cart.userId, userId)))

    if( existing.length === 0){
        throw new Error("Unauthorized or cart item not found."); 
    }

    await db.update(cafe1316Cart)
    .set({amount, updatedAt: new Date().toISOString(),})
    .where(eq(cafe1316Cart.id, cartItemId));
}

export const deleteCartItem = async(cartItemId: number, userId: string) => {
    const existing = await db.select().from(cafe1316Cart).where(and(eq(cafe1316Cart.id, cartItemId), eq(cafe1316Cart.userId, userId)))

    if (existing.length === 0){
        throw new Error("Unauthorized or cart item not found.");
    }

    await db.delete(cafe1316Cart).where(eq(cafe1316Cart.id, cartItemId));
}
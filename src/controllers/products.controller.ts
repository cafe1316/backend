import type { Request, Response } from "express";
import { getPopularProducts, searchProducts } from "../services/productService";

export const getPopularProductsController = async (req: Request, res: Response) => {
    const products = await getPopularProducts();
    res.json({ products });
};

export const searchProductsController = async (req: Request, res: Response) => {
    const { query } = req.query;
    const products = await searchProducts(query as string);
    res.json({ products });
};
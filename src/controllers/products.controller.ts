import type { Request, Response } from "express";
import { getPopularProducts, getProductByID, searchProducts, getPaginatedProducts } from "../services/productService";

export const getPopularProductsController = async (req: Request, res: Response) => {
    const products = await getPopularProducts();
    res.json({ products });
};

export const searchProductsController = async (req: Request, res: Response) => {
    const name = req.query.name as string;
    const baked = req.query.baked as string;
    const priceFrom = req.query.priceFrom ? 
        parseFloat(Array.isArray(req.query.priceFrom) ? req.query.priceFrom[0]?.toString() : req.query.priceFrom.toString()) : 
        undefined;
    const priceTo = req.query.priceTo ? 
        parseFloat(Array.isArray(req.query.priceTo) ? req.query.priceTo[0]?.toString() : req.query.priceTo.toString()) : 
        undefined;
    const sort = (req.query.sort as "asc" | "desc") || "asc";
    const page = parseInt(req.query.page as string) || 1;
    const num = parseInt(req.query.num as string) || 20;
    const products = await searchProducts({
        name,
        baked,
        priceFrom,
        priceTo,
        sort,
        page,
        num
    });
    res.json({ products });
};

export const getPaginatedProductsController = async (req: Request, res: Response) => {
    const page = parseInt(req.query.page as string) || 1;
    const num = parseInt(req.query.num as string) || 20;
    const products = await getPaginatedProducts(page, num);
    res.json({ products });
};

export const getProductByIDController = async (req: Request, res: Response) => {
    const id = parseInt(req.params.id);
    const products = await getProductByID(id);
    res.json({ products });
};
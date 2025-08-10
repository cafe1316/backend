// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, getProductByIDController, searchProductsController } from "../controllers/products.controller";
import { loginController, registerNewUserController, googleLoginController } from "../controllers/users.controller";
import {addCartController, getCartController, updateCartController, deleteCartController} from "../controllers/cart.controller";
import  {authMiddleware}  from "../middleware/auth";


const router = Router();

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/products", searchProductsController);

router.get("/v1/products/:id", getProductByIDController);

router.post("/v1/user", registerNewUserController);

router.post("/v1/user/login", loginController);

router.post("v1/user/google", googleLoginController)

router.post("/v1/cart/", authMiddleware, addCartController);

router.get("/v1/cart/", authMiddleware, getCartController);

router.put("v1/cart/:id", authMiddleware, updateCartController);

router.delete("v1/cart/:id", authMiddleware, deleteCartController);

export default router;
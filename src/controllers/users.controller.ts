import type { Request, Response } from "express";
import { registerNewUser } from "../services/userService1";

export const registerNewUserController = async (req: Request, res: Response) => {
    const body = req.body;
    // body is an object with key & value
    // username, email, password, confirmPassword
    try {
        await registerNewUser(body)
    } catch (error: any) {
        // console.log(error.message);
        res.json({status: 400, msg: error.message})
        return;
    }
    res.json({status: 200, data: "OK"})
    
}
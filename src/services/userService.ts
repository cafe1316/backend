import db from "../db";
import { cafe1316Users } from "../../drizzle/schema";
import { eq } from "drizzle-orm";
import bcrypt from "bcrypt";
import jwt from "jsonwebtoken";
import {v4 as uuidv4} from 'uuid';
import { firebaseAdmin } from "../bootstrap/firebaseAdmin";

const secret: string = process.env.JWT_SECRET || "";


interface RegisterForm {
  username: string;
  password: string;
  confirmPassword: string;
  email: string;
}

interface LoginForm {
  username: string;
  password: string;
}

export const registerNewUser = async (params: RegisterForm):Promise<string> => {
  if (
    !params.username ||
    !params.email ||
    !params.password ||
    !params.confirmPassword
  ) {
    // res.json({status: 400, msg: "username or email or password or confirmPassword parameter is missing"})
    throw new Error(
      "username or email or password or confirmPassword parameter is missing"
    );
  }
  // check if password matches with confirmPassword
  const { username, email, password, confirmPassword } = params;
  if (password !== confirmPassword) {
    throw new Error("password does not match!")
  }
  // check if this user already exists
  const res = await db.select().from(cafe1316Users).where(eq(cafe1316Users.username, username))
  if (res.length > 0) {
    throw new Error("user already exists!")
  }

  // password needs hashing
  const hashedPassword = await bcrypt.hash(password, 10);
  const generatedUuid = uuidv4();

  // create a new user
  const registerResult = await db.insert(cafe1316Users).values({
    uuid: generatedUuid,
    username, 
    password: hashedPassword, 
    email
  })
  
  const token = jwt.sign(
    { userId: generatedUuid, email: email },  // payload
    secret,
    { expiresIn: '1h' }                     // 可选：token 有效期
  );
  return token;

};

export const login = async (params: LoginForm):Promise<any> => {
  if (!params.username || !params.password){
    throw new Error ("username and password are required.");
  }

  const {username, password} = params;
  const res = await db.select().from(cafe1316Users).where(eq(cafe1316Users.username, username))
  if (res.length == 0) {
    throw new Error("user does not exist.");
  }

  let userinfo = res[0];
  if (!userinfo.password) {
    throw new Error("this account has no local password. please sign in with google or set a password first.");
  }

  // bcrypt.compare 需要 await
  const ok = await bcrypt.compare(password, userinfo.password);
  if (!ok) {
    throw new Error("username and password not match.");
  }
  
  //sign jwt
  const token = jwt.sign(
    { userId: username, email: userinfo.email },  // payload
    secret,
    { expiresIn: '1h' }                     // 可选：token 有效期
  );
  return token;
};

type LoginResult = {
  token: string;
  user: { uuid: string; username: string | null; email: string; isSocialLogin: boolean };
};

export const googleLogin = async (idToken: string) => {
  if (!idToken){
    throw new Error("missing google id token");
  }

  const decoded = await firebaseAdmin.auth().verifyIdToken(idToken).catch(()=>null);
  if (!decoded){
    throw new Error("Invalid google id token");
  }

  const email = decoded.email as string;
  const emailVerified = !!decoded.email_verified;

  if(!email){
    throw new Error("google account has no email");
  }

  if(!emailVerified){
    throw new Error("please verify your google email before login");
  }

  const existing = await db.select().from(cafe1316Users).where(eq(cafe1316Users.email, email));
  let user = existing[0];

  if(!user){
    const generatedUuid = uuidv4();
    const inserted = await db.insert(cafe1316Users).values({
      uuid: generatedUuid,
      username: null,          // 先为空：Google 登录不依赖 username
      password: null,          // 无本地密码
      email: email,
      isSocialLogin: true,
    }).returning();

    user = inserted[0];
  }else if (!user.isSocialLogin){
    await db.update(cafe1316Users).set({isSocialLogin: true}).where(eq(cafe1316Users.id, user.id));

    const refreshed = await db.select().from(cafe1316Users).where(eq(cafe1316Users.email, email));
    user = refreshed[0];
  }

  const token = jwt.sign(
    { userId: user.uuid, email: user.email },
    secret,
    { expiresIn: "1h" }
  );

  return {
    token,
    user: {
      uuid: user.uuid,
      username: user.username,       // 可能为 null（首登时）
      email: user.email,
      isSocialLogin: true,
    },
  };
}
"use client";

import Link from "next/link";

export default function Header() {
  return (
    <header className="border-b bg-white h-14 flex items-center px-4">
      <div className="flex items-center gap-4 w-full lg:max-w-screen mx-auto">
        <Link href="/feed" className="text-lg font-bold hover:opacity-80">
          POSTS
        </Link>

        <nav className="flex gap-4 ml-auto">
          <Link href="/profile/me" className="hover:text-blue-600">
            Profile
          </Link>
          <Link href="/post/create" className="hover:text-blue-600">
            New Post
          </Link>
        </nav>
      </div>
    </header>
  );
}
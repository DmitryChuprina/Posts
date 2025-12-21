'use client';

import Image, { ImageProps } from 'next/image';

const isDev = process.env.NODE_ENV === 'development';

export default function AppImage(props: ImageProps) {
    return (
        // eslint-disable-next-line jsx-a11y/alt-text
        <Image
            {...props}
            unoptimized={isDev ? true : props.unoptimized}
        />
    );
}
export interface CompressOptions {
    maxDimension?: number;
    quality?: number;
    aspectRatio?: number;
}

/**
 * Recorta (centralizado), redimensiona e comprime uma imagem no navegador (canvas)
 * e devolve um novo File em JPEG.
 *
 * Observações:
 * - PNG/WEBP com transparência ganham fundo branco (JPEG não tem transparência).
 * - Formatos que o navegador não decodifica (ex.: HEIC) lançam erro: use try/catch.
 */
export async function compressImage(
    file: File,
    { maxDimension = 1600, quality = 0.8, aspectRatio }: CompressOptions = {},
): Promise<File> {
    const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' });

    let sx = 0;
    let sy = 0;
    let sw = bitmap.width;
    let sh = bitmap.height;
    let cropped = false;

    if (aspectRatio) {
        const sourceRatio = bitmap.width / bitmap.height;

        if (sourceRatio > aspectRatio) {
            sw = Math.round(bitmap.height * aspectRatio);
            sx = Math.round((bitmap.width - sw) / 2);
            cropped = true;
        } else if (sourceRatio < aspectRatio) {
            sh = Math.round(bitmap.width / aspectRatio);
            sy = Math.round((bitmap.height - sh) / 2);
            cropped = true;
        }
    }

    const scale = Math.min(1, maxDimension / Math.max(sw, sh));
    const width = Math.round(sw * scale);
    const height = Math.round(sh * scale);

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;

    const ctx = canvas.getContext('2d');
    if (!ctx) {
        bitmap.close();
        return file;
    }

    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, width, height);
    ctx.drawImage(bitmap, sx, sy, sw, sh, 0, 0, width, height);
    bitmap.close();

    const blob = await new Promise<Blob | null>((resolve) =>
        canvas.toBlob(resolve, 'image/jpeg', quality),
    );

    if (!blob) {
        return file;
    }

    const transformed = cropped || scale < 1;
    if (!transformed && blob.size >= file.size) {
        return file;
    }

    const baseName = file.name.replace(/\.[^.]+$/, '');
    return new File([blob], `${baseName}.jpg`, { type: 'image/jpeg' });
}
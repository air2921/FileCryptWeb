import { format } from "date-fns";

export function resizeImage(file: File, width: number, height: number): Promise<File> {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);

        reader.onload = event => {
            const img = new Image();
            img.src = event.target?.result as string;

            img.onload = () => {
                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                if (ctx) {
                    ctx.drawImage(img, 0, 0, width, height);
                    canvas.toBlob(blob => {
                        if (blob) {
                            resolve(new File([blob], file.name, { type: file.type }));
                        } else {
                            reject(new Error('Canvas is empty'));
                        }
                    }, file.type);
                } else {
                    reject(new Error('Could not get canvas context'));
                }
            };

            img.onerror = error => reject(error);
        };

        reader.onerror = error => reject(error);
    });
};

export function dateFormate(date: string): string {
    const originalDate = new Date(date);
    const formattedDate = format(originalDate, "dd.MM.yyyy 'at' h:mm:ss a");

    return formattedDate;
}

export function lineFormate(message: string): string {
    const splitter = '|NEW_LINE|'
    const lines = message.split(splitter);
    return lines.join('\n');
}
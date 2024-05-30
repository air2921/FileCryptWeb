export function handleLoadMore(count: number, currentSkip: number, onSkipChange: (count: number) => void) {
    const newSkip = currentSkip + count;
    onSkipChange(newSkip);
}

export function handleLoadLess(count: number, currentSkip: number, onSkipChange: (newSkip: number) => void) {
    const newSkip = Math.max(0, currentSkip - count);
    onSkipChange(newSkip);
}
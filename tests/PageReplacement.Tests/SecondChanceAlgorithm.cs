public class SecondChanceAlgorithm
{
    private int[] frames;
    private int[] referenceBits;
    private int clockPointer;
    private int frameSize;

    public SecondChanceAlgorithm(int frameSize)
    {
        this.frameSize = frameSize;
        this.frames = new int[frameSize];
        this.referenceBits = new int[frameSize];
        this.clockPointer = 0;

        for (int i = 0; i < frameSize; i++) {
            frames[i] = -1;
            referenceBits[i] = 0;
        }
    }

    public string AccessPage(int page)
    {
        for (int i = 0; i < frameSize; i++) {
            if (frames[i] == page) {
                referenceBits[i] = 1; // Cho bit = 1
                return "HIT";
            }
        }

        while (true) {
            if (frames[clockPointer] == -1 || referenceBits[clockPointer] == 0) {
                frames[clockPointer] = page;
                referenceBits[clockPointer] = 1;
                clockPointer = (clockPointer + 1) % frameSize; // Xoay vòng con trỏ
                return "FAULT";
            }
            referenceBits[clockPointer] = 0;
            clockPointer = (clockPointer + 1) % frameSize;
        }
    }
}

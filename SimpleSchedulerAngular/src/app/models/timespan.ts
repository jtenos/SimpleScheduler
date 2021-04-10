export default class TimeSpan {

    constructor(public hours: number, public minutes: number) {

    }

    // Must be in format hhmm
    static parse(s: string): TimeSpan {
        const hours = parseInt(s.substring(0, 2), 10);
        const minutes = parseInt(s.substring(2, 4), 10);
        return new TimeSpan(hours, minutes);
    }

    static fromObject(o: {hours: number, minutes: number}) {
        return new TimeSpan(o.hours, o.minutes);
    }

    toNumber(): number {
        return this.hours * 100 + this.minutes;
    }

    asFormattedTimeOfDay(): string {
        return `${this.hours}:${zeroPad(this.minutes)}`;
    }
    
    asFormattedTimeSpan(): string {
        if (this.hours === 1) { return "every hour"; }
        if (this.hours > 1) { return `every ${this.hours} hours`; }
        if (this.minutes === 1) { return "every minute"; }
        if (this.minutes > 1) { return `every ${this.minutes} minutes`; }
        return "unknown";
    }
}

function zeroPad(num: number): string {
    if (num < 10) { return "0" + num.toString(); }
    return num.toString();
}

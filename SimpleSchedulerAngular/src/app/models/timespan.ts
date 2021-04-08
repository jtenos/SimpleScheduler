export default class TimeSpan {

    constructor(public hours: number, public minutes: number) {

    }

    // Must be in format hhmm
    static parse(s: string): TimeSpan {
        const hours = parseInt(s.substring(0, 2), 10);
        const minutes = parseInt(s.substring(2, 4), 10);
        return new TimeSpan(hours, minutes);
    }

    toString(): string {
        return `${this.hours}H ${this.minutes}M`;
    }

    toNumber(): number {
        return this.hours * 100 + this.minutes;
    }
}

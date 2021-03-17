export default class Schedule {
    constructor(
        public scheduleID: number,
        public isActive: boolean,
        public workerID: number,
        public sunday: boolean,
        public monday: boolean,
        public tuesday: boolean,
        public wednesday: boolean,
        public thursday: boolean,
        public friday: boolean,
        public saturday: boolean,
        public oneTime: boolean,
        public timeOfDayUTC?: any,// TODO: TimeSpan data type
        public recurTime?: any,// TODO: TimeSpan data type
        public recurBetweenStartUTC?: any,// TODO: TimeSpan data type
        public recurBetweenEndUTC?: any// TODO: TimeSpan data type
    ){}
}
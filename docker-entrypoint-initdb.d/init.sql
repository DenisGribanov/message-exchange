CREATE DATABASE ExchangeMessage
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

CREATE TABLE public."Messages"
(
    "Id" bigint NOT NULL,
    "Number" bigint NOT NULL,
    "Text" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL,
    PRIMARY KEY ("Id")
);

ALTER TABLE IF EXISTS public."Messages"
    OWNER to postgres;
CREATE DATABASE "ExchangeMessage"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;


ALTER DATABASE "ExchangeMessage" OWNER TO postgres;

\connect "ExchangeMessage"



ALTER SCHEMA public OWNER TO postgres;
--

CREATE TABLE public."Messages" (
    "Id" bigint NOT NULL,
    "Number" bigint NOT NULL,
    "Text" text NOT NULL,
    "DateCreated" timestamp with time zone NOT NULL
);


ALTER TABLE public."Messages" OWNER TO postgres;



ALTER TABLE public."Messages" ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Messages_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE ONLY public."Messages"
    ADD CONSTRAINT "Messages_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 3316 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2025-02-07 12:32:18

--
-- PostgreSQL database dump complete
--

